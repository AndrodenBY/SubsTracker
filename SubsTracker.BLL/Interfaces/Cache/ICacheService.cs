namespace SubsTracker.BLL.Interfaces.Cache;

public interface ICacheService
{
    Task<TValue?> CacheDataWithLock<TValue>(string cacheKey, TimeSpan expirationTime, Func<Task<TValue>> dataFactory,
        CancellationToken cancellationToken) where TValue : class;
}