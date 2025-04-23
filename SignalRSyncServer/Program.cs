using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Security.Claims;
using System.Security.Principal;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Web;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddSignalR();
builder.Services.AddAuthentication().AddScheme<IdentityAuthenticationOptions, IdentityAuthenticationHandler>(IdentityAuthenticationHandler.SchemeName, options => { });
builder.Services.AddSingleton<IIdentityAuthenticationInfoRepository, IdentityAuthenticationInfoRepository>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapHub<SignalRSyncHub>("SyncHub");
app.MapHub<IdentityAuthenticationInfosObserverHub>("ObserverHub");

app.Run();

public interface ISignalRSync
{
    Task SyncAsync(SyncParameters parameters);
}

public record SyncParameters(string methodName, string? groupName, object?[] args);

[Authorize]
public class SignalRSyncHub : Hub<ISignalRSync>, ISignalRSync
{
    private readonly IHubContext<IdentityAuthenticationInfosObserverHub, IIdentityAuthenticationInfosObserver> _observer;
    private readonly IIdentityAuthenticationInfoRepository _repository;
    public SignalRSyncHub(IIdentityAuthenticationInfoRepository repository,IHubContext<IdentityAuthenticationInfosObserverHub, IIdentityAuthenticationInfosObserver> observer)
    {
        _observer = observer;
        _repository = repository;
    }
    public override async Task OnConnectedAsync()
    {
        var httpContext = Context.GetHttpContext()!;
        var info = new IdentityAuthenticationInfo(Context.ConnectionId, BitConverter.ToInt32(httpContext.Connection.RemoteIpAddress!.GetAddressBytes()), httpContext.Connection.RemotePort, (httpContext.User as IdentityUser)!.IdentityInfo);
        await _repository.AddAsync(Context.ConnectionId, info);
        await _observer.Clients.All.ConnectedAsync(info);
    }
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await _repository.RemoveAsync(Context.ConnectionId);
        await _observer.Clients.All.DisconnectedAsync(Context.ConnectionId);
    }
    public Task SyncAsync(SyncParameters parameters)
    {
        var httpContext = Context.GetHttpContext()!;
        var group = httpContext.User.FindFirstValue(ClaimTypes.Role)!;
        return Clients.GroupExcept(group,Context.ConnectionId).SyncAsync(parameters);
    }
}

public class IdentityAuthenticationOptions:AuthenticationSchemeOptions
{
    public string HeaderName { get; set; } = IdentityAuthenticationHandler.SchemeName;
}
public record IdentityInfo(string clientName,string groupName);
public class IdentityAuthenticationHandler : AuthenticationHandler<IdentityAuthenticationOptions>
{
    public const string SchemeName = "Identity";

    public IdentityAuthenticationHandler(IOptionsMonitor<IdentityAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder) : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var userStr = Context.Request.Headers[Options.HeaderName].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(userStr))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }
        var option = new JsonSerializerOptions(JsonSerializerOptions.Default);
        option.PropertyNameCaseInsensitive = true;
        var info = JsonSerializer.Deserialize<IdentityInfo>(HttpUtility.UrlDecode(userStr),option );
        if (info == null || string.IsNullOrWhiteSpace(info.groupName) || string.IsNullOrWhiteSpace(info.clientName))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }
        var identity = new IdentityUser(info);
        var ticket = new AuthenticationTicket(identity, SchemeName);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
public class IdentityUser : ClaimsPrincipal
{
    public IdentityUser(IdentityInfo identityInfo) : base(new ClaimsIdentity([
            new Claim(ClaimTypes.Name,identityInfo.clientName),
            new Claim(ClaimTypes.Role,identityInfo.groupName),
        ], IdentityAuthenticationHandler.SchemeName))
    {
        IdentityInfo = identityInfo;
    }
    public IdentityInfo IdentityInfo { get; }
}
public record IdentityAuthenticationInfo(string connectionId, int ip, int port, IdentityInfo identityInfo);
public interface IIdentityAuthenticationInfoRepository
{
    Task AddAsync(string id, IdentityAuthenticationInfo info);
    Task RemoveAsync(string id);
    Task<IEnumerable<IdentityAuthenticationInfo>> GetAllAsync();
}
public class IdentityAuthenticationInfoRepository : IIdentityAuthenticationInfoRepository
{
    private readonly ConcurrentDictionary<string, IdentityAuthenticationInfo> _repository=new ConcurrentDictionary<string, IdentityAuthenticationInfo>();
    public Task AddAsync(string id, IdentityAuthenticationInfo info)
    {
        _repository.TryAdd(id, info);
        return Task.CompletedTask;
    }

    public Task<IEnumerable<IdentityAuthenticationInfo>> GetAllAsync()
    {
        return Task.FromResult(_repository.Select(a=>a.Value));
    }

    public Task RemoveAsync(string id)
    {
        _repository.TryRemove(id,out _);
        return Task.CompletedTask;
    }
}

public interface IIdentityAuthenticationInfosObserver
{
    Task ConnectedAsync(IdentityAuthenticationInfo info);
    Task DisconnectedAsync(string id);
}
public class IdentityAuthenticationInfosObserverHub : Hub<IIdentityAuthenticationInfosObserver>
{
    private readonly IIdentityAuthenticationInfoRepository _repository;
    public IdentityAuthenticationInfosObserverHub(IIdentityAuthenticationInfoRepository repository)
    {
        _repository = repository;
    }
    public Task<IEnumerable<IdentityAuthenticationInfo>> GetConnectedAllAsync()
    {
        return _repository.GetAllAsync();
    }
}