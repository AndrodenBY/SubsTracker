using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SubsTracker.BLL.Interfaces.Cache;
using SubsTracker.BLL.Json;

namespace SubsTracker.BLL.Services.Cache;

public class CacheAccessService(IDistributedCache cache, ILogger<CacheAccessService> logger) : ICacheAccessService
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
    
    public async Task RemoveData(string cacheKey, CancellationToken cancellationToken)
    {
        await cache.RemoveAsync(cacheKey, cancellationToken);
    }
}
