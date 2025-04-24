using Microsoft.AspNetCore.Authentication;

public class IdentityAuthenticationOptions:AuthenticationSchemeOptions
{
    public string HeaderName { get; set; } = IdentityAuthenticationHandler.SchemeName;
}
