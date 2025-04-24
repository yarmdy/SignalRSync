using System.Security.Claims;

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
