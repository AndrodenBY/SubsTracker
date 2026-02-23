using SubsTracker.BLL.RedisSettings;

namespace SubsTracker.BLL.Interfaces.Cache;

/// <summary>
/// Interface for caching service with distributed lock
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Retrieves data from cache or populates it using factory with RedLock.
    /// Ensures only one thread creates the data by checking the cache before and after acquiring the lock.
    /// </summary>
    /// <remarks>
    /// Prevents "Cache Stampede" by ensuring that only one concurrent request executes
    /// the <paramref name="dataFactory"/> when the cache expires 
    /// </remarks>
    /// <typeparam name="TValue">Type of the cached object, must be a reference type</typeparam>
    /// <param name="cacheKey">Unique identifier for the cached data</param>
    /// <param name="dataFactory">Asynchronous delegate for fetching the data if it's missing from the cache</param>
    /// <param name="cancellationToken">Token used to cancel the operation if requested</param>
    /// <param name="expirationTime">Time in which the data should remain in the cache</param>
    /// <returns>The cached or freshly fetched instance of <typeparamref name="TValue"/>; otherwise, <c>null</c></returns>
    Task<TValue?> CacheDataWithLock<TValue>(
        string cacheKey, 
        Func<Task<TValue?>> dataFactory, 
        CancellationToken cancellationToken,
        TimeSpan? expirationTime = null) where TValue : class;
}
