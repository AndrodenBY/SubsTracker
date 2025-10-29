namespace SubsTracker.BLL.Interfaces.Cache;

public interface ICacheAccessService
{
    Task<TValue?> GetData<TValue>(string cacheKey, CancellationToken cancellationToken);
    Task SetData<TValue>(string cacheKey, TValue value, TimeSpan expirationTime, CancellationToken cancellationToken);
    Task RemoveData(List<string> cacheKeys, CancellationToken cancellationToken);
}