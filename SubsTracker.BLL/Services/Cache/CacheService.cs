using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using RedLockNet;
using SubsTracker.BLL.Interfaces.Cache;
using SubsTracker.BLL.RedisSettings;

namespace SubsTracker.BLL.Services.Cache;

/// <summary>
/// Implementation of <see cref="ICacheService"/> using <see cref="IDistributedCache"/> 
/// for storage and <see cref="IDistributedLockFactory"/>(RedLock) for synchronization
/// </summary>
public class CacheService(IDistributedCache cache, ILogger<CacheService> logger, IDistributedLockFactory lockFactory)
    : CacheAccessService(cache, logger), ICacheService
{
    /// <inheritdoc />
    public async Task<TValue?> CacheDataWithLock<TValue>(
        string cacheKey,
        Func<Task<TValue?>>? dataFactory,
        CancellationToken cancellationToken,
        TimeSpan? expirationTime = null) where TValue : class
    {
        expirationTime ??= RedisConstants.ExpirationTime;
        
        var data = await GetData<TValue>(cacheKey, cancellationToken);
        if (data is not null || dataFactory is null)
        {
            return data;
        }

        return await LockAndPopulate(cacheKey, expirationTime.Value, dataFactory, cancellationToken);
    }

    /// <summary>
    /// Orchestrates the distributed lock lifecycle and handles cache population
    /// </summary>
    /// <remarks>
    /// If the lock is acquired - checks the cache again before executing the factory
    /// If the lock is not acquired - waits for the result from the thread that currently holds the lock
    /// </remarks>
    private async Task<TValue?> LockAndPopulate<TValue>(string cacheKey,
        TimeSpan expirationTime,
        Func<Task<TValue?>> dataFactory,
        CancellationToken cancellationToken) where TValue : class
    {
        var lockKey = RedisKeySetter.SetLockKey(cacheKey);
        
        await using var redLock = await lockFactory.CreateLockAsync(
            lockKey,
            RedisConstants.LockExpiry,
            RedisConstants.LockWaitTime,
            RedisConstants.LockRetryTime,
            cancellationToken
        );

        if (!redLock.IsAcquired)
        {
            return await WaitForCachedData<TValue>(cacheKey, RedisConstants.LockWaitTime, cancellationToken);
        }

        var cacheCheck = await GetData<TValue>(cacheKey, cancellationToken)
            ?? await ExecuteFactory(cacheKey, expirationTime, dataFactory, cancellationToken);
        
        return cacheCheck;
    }

    /// <summary>
    /// Executes the expensive data retrieval and stores the result in the distributed cache.
    /// </summary>
    private async Task<TValue?> ExecuteFactory<TValue>(string cacheKey,
        TimeSpan expirationTime,
        Func<Task<TValue?>> dataFactory,
        CancellationToken cancellationToken) where TValue : class
    {
        logger.LogInformation("Lock acquired. Executing expensive data factory for key: {CacheKey}", cacheKey);
        
        var data = await dataFactory();
        if (data is not null)
        {
            await SetData(cacheKey, data, expirationTime, cancellationToken);
        }

        return data;
    }

    /// <summary>
    /// Invoked when thread fails to acquire the lock 
    /// Waits while the lock holder is expected to populate the cache
    /// </summary>
    /// <param name="cacheKey">Key to check after the delay</param>
    /// <param name="retryDelay">Time to wait before checking the cache again</param>
    /// /// <param name="cancellationToken">Token used to cancel the operation if requested</param>
    private async Task<TValue?> WaitForCachedData<TValue>(string cacheKey, TimeSpan retryDelay,
        CancellationToken cancellationToken) where TValue : class
    {
        await Task.Delay(retryDelay, cancellationToken);
        return await GetData<TValue>(cacheKey, cancellationToken);
    }
}
