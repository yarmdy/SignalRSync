using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Web;

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
