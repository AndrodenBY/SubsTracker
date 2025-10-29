namespace SubsTracker.BLL.RedisSettings;

public static class RedisKeySetter
{
    public static string SetCacheKey<T>(Guid id) => $"{id}:{typeof(T).Name}";
    public static string SetCacheKey(Guid id, string suffix) => $"{id}:{suffix}";
    public static string SetLockKey(string cacheKey) => $"lock:{cacheKey}";
}
