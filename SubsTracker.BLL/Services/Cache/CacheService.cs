using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using RedLockNet;
using SubsTracker.BLL.Interfaces.Cache;

namespace SubsTracker.BLL.Services.Cache;

public class CacheService(IDistributedCache cache, ILogger<CacheService> logger, IDistributedLockFactory lockFactory)
    : CacheAccessService(cache, logger), ICacheService
{
    public async Task<TValue?> CacheDataWithLock<TValue>(string cacheKey, TimeSpan expirationTime,
        Func<Task<TValue>> dataFactory, CancellationToken cancellationToken) where TValue : class
    {
        var cachedData = await GetData<TValue>(cacheKey, cancellationToken);
        if (cachedData is not null)
        {
            return cachedData;
        }
        
        var lockKey = $"lock:{cacheKey}";
        var lockExpiry = TimeSpan.FromSeconds(10);
        var waitTime = TimeSpan.FromSeconds(5);
        var retryTime = TimeSpan.FromMilliseconds(200);
        
        while (true)
        {
            using var redLock =
                await lockFactory.CreateLockAsync(lockKey, lockExpiry, waitTime, retryTime, cancellationToken);

            if (redLock.IsAcquired)
            {
                return await EnsureCached(cacheKey, expirationTime, dataFactory, cancellationToken);
            }
            
            var dataAfterWait = await WaitForCachedData<TValue>(cacheKey, waitTime, cancellationToken);
            if (dataAfterWait is not null)
            {
                return dataAfterWait;
            }
        }
    }

    private async Task<TValue?> EnsureCached<TValue>(string cacheKey, TimeSpan expirationTime,
        Func<Task<TValue>> dataFactory,
        CancellationToken cancellationToken) where TValue : class
    {
        var finalCachedData = await GetData<TValue>(cacheKey, cancellationToken);
        if (finalCachedData is not null) return finalCachedData;

        logger.LogInformation("Lock acquired. Executing expensive data factory for key: {CacheKey}", cacheKey);

        var newCachedData = await dataFactory();
        if (newCachedData is not null)
        {
            await SetData(cacheKey, newCachedData, expirationTime, cancellationToken);
        }
        return newCachedData;
    }

    private async Task<TValue?> WaitForCachedData<TValue>(string cacheKey, TimeSpan retryDelay,
        CancellationToken cancellationToken) where TValue : class
    {
        await Task.Delay(retryDelay, cancellationToken);
        return await GetData<TValue>(cacheKey, cancellationToken);
    }
}
