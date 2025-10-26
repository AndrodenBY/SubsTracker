namespace SubsTracker.BLL.Interfaces.Cache;

public interface ICacheService
{
    Task<TValue?> GetData<TValue>(string cacheKey, CancellationToken cancellationToken);
    Task SetData<TValue>(string cacheKey, TValue value, TimeSpan expirationTime, CancellationToken cancellationToken);
    Task RemoveData(string cacheKey, CancellationToken cancellationToken);
    Task<TValue?> CacheDataWithLock<TValue>(string cacheKey, TimeSpan expirationTime, Func<Task<TValue>> dataFactory,
        CancellationToken cancellationToken) where TValue : class;
}
