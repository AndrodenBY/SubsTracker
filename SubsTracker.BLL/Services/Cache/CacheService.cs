using SubsTracker.BLL.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SubsTracker.BLL.Interfaces.Cache;
using Medallion.Threading;

namespace SubsTracker.BLL.Services.Cache;

public class CacheService(IDistributedCache cache, ILogger<CacheService> logger, IDistributedLockProvider lockProvider) : ICacheService
{
    public async Task<TValue?> GetData<TValue>(string cacheKey, CancellationToken cancellationToken)
    {
        var cachedData = await cache.GetStringAsync(cacheKey, cancellationToken);
        logger.LogInformation("{DataState}: {CacheKey}", cachedData is null ? "Cache miss" : "Cache hit", cacheKey);
        
        return cachedData is null ? default :  JsonConvert.DeserializeObject<TValue>(cachedData, NewtonsoftJsonSettings.Default);
    }

    public async Task SetData<TValue>(string cacheKey, TValue value, TimeSpan expirationTime, CancellationToken cancellationToken)
    {
        var cacheEntryOptions = new DistributedCacheEntryOptions
        {
            SlidingExpiration = expirationTime
        };

        var serializedValue = JsonConvert.SerializeObject(value, NewtonsoftJsonSettings.Default);
        await cache.SetStringAsync(cacheKey, serializedValue, cacheEntryOptions, cancellationToken);
    }
    
    public async Task<TValue?> CacheDataWithLock<TValue>(string cacheKey, TimeSpan expirationTime, Func<Task<TValue>> dataFactory, 
        CancellationToken cancellationToken) where TValue : class
    {
        var cachedData = await GetData<TValue>(cacheKey, cancellationToken);
        if (cachedData is not null)
        {
            return cachedData;
        }

        var lockKey = $"lock:{cacheKey}";
        var distributedLock = lockProvider.CreateLock(lockKey);
        
        while (true)
        {
            await using (var handle = await distributedLock.TryAcquireAsync(TimeSpan.FromSeconds(5), cancellationToken))
            {
                if (handle is null)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(200), cancellationToken);
                
                    var dataAfterWait = await GetData<TValue>(cacheKey, cancellationToken);
                    if (dataAfterWait is not null)
                    {
                        return dataAfterWait;
                    }
                    continue;
                }
                
                return await EnsureCached(cacheKey, expirationTime, dataFactory, cancellationToken);
            }
        }
    }
    
    public async Task RemoveData(string cacheKey, CancellationToken cancellationToken)
    {
        await cache.RemoveAsync(cacheKey, cancellationToken );
    }
    
    private async Task<TValue?> EnsureCached<TValue>(string cacheKey, TimeSpan expirationTime, Func<Task<TValue>> dataFactory, 
        CancellationToken cancellationToken) where TValue : class
    {
        var finalCachedData = await GetData<TValue>(cacheKey, cancellationToken);
        if (finalCachedData is not null)
        {
            return finalCachedData;
        }

        logger.LogInformation("Lock acquired. Executing expensive data factory for key: {CacheKey}", cacheKey);
        
        var newCachedData = await dataFactory();
        
        if (newCachedData is not null)
        {
            await SetData(cacheKey, newCachedData, expirationTime, cancellationToken);
        }
        
        return newCachedData;
    }
}
