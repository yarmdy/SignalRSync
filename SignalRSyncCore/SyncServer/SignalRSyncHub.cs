using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

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
        await Groups.AddToGroupAsync(Context.ConnectionId,info.identityInfo.groupName);
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
