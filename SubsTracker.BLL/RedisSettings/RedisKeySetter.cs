namespace SubsTracker.BLL.RedisSettings;

public static class RedisKeySetter
{
    public static string SetCacheKey<T>(Guid id)
    {
        return $"{id}:{typeof(T).Name}";
    }

    public static string SetCacheKey(Guid id, string suffix)
    {
        return $"{id}:{suffix}";
    }
    
    public static string SetCacheKey<T>(string stringIdentifier)
    {
        return $"{stringIdentifier}:{typeof(T).Name}";
    }

    public static string SetLockKey(string cacheKey)
    {
        return $"lock:{cacheKey}";
    }
}
