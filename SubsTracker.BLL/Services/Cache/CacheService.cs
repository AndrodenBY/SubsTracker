using SubsTracker.BLL.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SubsTracker.BLL.Interfaces.Cache;

namespace SubsTracker.BLL.Services.Cache;

public class CacheService(IDistributedCache cache, ILogger<CacheService> logger) : ICacheService
{
    public T? GetData<T>(string cacheKey)
    {
        var data = cache.GetString(cacheKey);
        logger.LogInformation(data is null ? $"Cache miss: {cacheKey}" : $"Cache hit: {cacheKey}");
        
        return data is null ? default :  JsonConvert.DeserializeObject<T>(data, NewtonsoftJsonSettings.Default);
    }

    public void SetData<T>(string cacheKey, T value, TimeSpan expiration)
    {
        var options = new DistributedCacheEntryOptions
        {
            SlidingExpiration = expiration
        };

        cache.SetString(cacheKey, JsonConvert.SerializeObject(value, NewtonsoftJsonSettings.Default), options);
    }

    public void RemoveData(string cacheKey)
    {
        cache.Remove(cacheKey);
    }
}
