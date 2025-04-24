public interface IIdentityAuthenticationInfoRepository
{
    Task AddAsync(string id, IdentityAuthenticationInfo info);
    Task RemoveAsync(string id);
    Task<IEnumerable<IdentityAuthenticationInfo>> GetAllAsync();
}
