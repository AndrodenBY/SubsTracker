using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using RedLockNet;
using SubsTracker.BLL.Interfaces.Cache;
using SubsTracker.BLL.RedisSettings;

namespace SubsTracker.BLL.Services.Cache;

public class CacheService(IDistributedCache cache, ILogger<CacheService> logger, IDistributedLockFactory lockFactory)
    : CacheAccessService(cache, logger), ICacheService
{
    public async Task<TValue?> CacheDataWithLock<TValue>(
        string cacheKey,
        TimeSpan expirationTime,
        Func<Task<TValue?>>? dataFactory,
        CancellationToken cancellationToken) where TValue : class
    {
        var data = await GetData<TValue>(cacheKey, cancellationToken);
        if (data is not null || dataFactory is null)
        {
            return data;
        }

        return await LockAndPopulate(cacheKey, expirationTime, dataFactory, cancellationToken);
    }

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

    private async Task<TValue?> WaitForCachedData<TValue>(string cacheKey, TimeSpan retryDelay,
        CancellationToken cancellationToken) where TValue : class
    {
        await Task.Delay(retryDelay, cancellationToken);
        return await GetData<TValue>(cacheKey, cancellationToken);
    }
}
