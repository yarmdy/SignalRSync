public interface IIdentityAuthenticationInfosObserver
{
    Task ConnectedAsync(IdentityAuthenticationInfo info);
    Task DisconnectedAsync(string id);
}
