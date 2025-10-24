namespace SubsTracker.BLL.Interfaces.Cache;

public interface ICacheService
{
    Task<T?> GetData<T>(string cacheKey, CancellationToken cancellationToken);
    Task SetData<T>(string cacheKey, T value, TimeSpan expiration, CancellationToken cancellationToken);
    Task RemoveData(string cacheKey, CancellationToken cancellationToken);
}
