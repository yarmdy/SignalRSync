using System.Collections.Concurrent;

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
