using Medallion.Threading;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RedLockNet;
using SubsTracker.BLL.Interfaces.Cache;
using SubsTracker.BLL.Json;

namespace SubsTracker.BLL.Services.Cache;

public class CacheService(IDistributedCache cache, ILogger<CacheService> logger, IDistributedLockFactory lockFactory)
    : ICacheService
{
    public async Task<TValue?> GetData<TValue>(string cacheKey, CancellationToken cancellationToken)
    {
        var cachedData = await cache.GetStringAsync(cacheKey, cancellationToken);
        logger.LogInformation("{DataState}: {CacheKey}", cachedData is null ? "Cache miss" : "Cache hit", cacheKey);

        return cachedData is null
            ? default
            : JsonConvert.DeserializeObject<TValue>(cachedData, NewtonsoftJsonSettings.Default);
    }

    public async Task SetData<TValue>(string cacheKey, TValue value, TimeSpan expirationTime,
        CancellationToken cancellationToken)
    {
        var cacheEntryOptions = new DistributedCacheEntryOptions
        {
            SlidingExpiration = expirationTime
        };

        var serializedValue = JsonConvert.SerializeObject(value, NewtonsoftJsonSettings.Default);
        await cache.SetStringAsync(cacheKey, serializedValue, cacheEntryOptions, cancellationToken);
    }

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

    public async Task RemoveData(string cacheKey, CancellationToken cancellationToken)
    {
        await cache.RemoveAsync(cacheKey, cancellationToken);
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
