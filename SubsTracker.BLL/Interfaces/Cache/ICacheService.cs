namespace SubsTracker.BLL.Interfaces.Cache;

/// <summary>
/// Interface for caching service with distributed lock
/// </summary>
public interface ICacheService : ICacheAccessService
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
    
    /// <summary>
    /// Invalidates the cache for a specific entity and its related dependencies
    /// </summary>
    /// <remarks>
    /// This method performs "intelligent invalidation" by automatically generating the primary cache key 
    /// based on the type <typeparamref name="TEntity"/> and the <paramref name="id"/>. It then optionally 
    /// removes any <paramref name="additionalKeys"/> in a single batch operation to ensure data consistency across the application
    /// </remarks>
    /// <typeparam name="TEntity">The type used to generate the primary cache key prefix</typeparam>
    /// <param name="id">The unique identifier of the entity to invalidate</param>
    /// <param name="cancellationToken">Token used to cancel the operation if requested</param>
    /// <param name="additionalKeys">Optional array of specific cache keys related to this entity that should also be removed</param>
    /// <returns>A task representing the asynchronous invalidation operation.</returns>
    Task InvalidateCache<TEntity>(Guid id, CancellationToken cancellationToken, params string[] additionalKeys);
    
    /// <summary>
    /// Invalidates the cache for a specific entity and its related dependencies. Overload with id type of string
    /// </summary>
    /// <remarks>
    /// This method performs "intelligent invalidation" by automatically generating the primary cache key 
    /// based on the type <typeparamref name="TEntity"/> and the <paramref name="id"/>. It then optionally 
    /// removes any <paramref name="additionalKeys"/> in a single batch operation to ensure data consistency across the application
    /// </remarks>
    /// <typeparam name="TEntity">The type used to generate the primary cache key prefix</typeparam>
    /// <param name="id">The unique identifier of the entity to invalidate</param>
    /// <param name="cancellationToken">Token used to cancel the operation if requested</param>
    /// <param name="additionalKeys">Optional array of specific cache keys related to this entity that should also be removed</param>
    /// <returns>A task representing the asynchronous invalidation operation.</returns>
    Task InvalidateCache<TEntity>(string id, CancellationToken cancellationToken, params string[] additionalKeys);
}
