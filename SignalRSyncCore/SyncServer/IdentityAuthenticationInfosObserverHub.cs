using Microsoft.AspNetCore.SignalR;

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