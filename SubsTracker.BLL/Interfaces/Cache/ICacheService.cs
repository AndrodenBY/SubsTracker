namespace SubsTracker.BLL.Interfaces.Cache;

public interface ICacheService
{
    T? GetData<T>(string cacheKey);
    void SetData<T>(string cacheKey, T value, TimeSpan expiration);
    void RemoveData(string cacheKey);
}
