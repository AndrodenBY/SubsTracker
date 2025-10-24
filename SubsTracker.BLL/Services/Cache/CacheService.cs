using SubsTracker.BLL.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SubsTracker.BLL.Interfaces.Cache;

namespace SubsTracker.BLL.Services.Cache;

public class CacheService(IDistributedCache cache, ILogger<CacheService> logger) : ICacheService
{
    public async Task<T?> GetData<T>(string cacheKey, CancellationToken cancellationToken)
    {
        var data = await cache.GetStringAsync(cacheKey, cancellationToken);
        logger.LogInformation("{DataState}: {CacheKey}", data is null ? "Cache miss" : "Cache hit", cacheKey);
        
        return data is null ? default :  JsonConvert.DeserializeObject<T>(data, NewtonsoftJsonSettings.Default);
    }

    public async Task SetData<T>(string cacheKey, T value, TimeSpan expiration, CancellationToken cancellationToken)
    {
        var options = new DistributedCacheEntryOptions
        {
            SlidingExpiration = expiration
        };

        var serializedValue = JsonConvert.SerializeObject(value, NewtonsoftJsonSettings.Default);
        await cache.SetStringAsync(cacheKey, serializedValue, options, cancellationToken);
    }

    public async Task RemoveData(string cacheKey, CancellationToken cancellationToken)
    {
        await cache.RemoveAsync(cacheKey, cancellationToken );
    }
}
