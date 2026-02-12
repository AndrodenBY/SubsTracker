using SubsTracker.BLL.Interfaces.Cache;

namespace SubsTracker.IntegrationTests.Configuration;

public class FakeCacheService : ICacheService
{
    public Task<TValue?> CacheDataWithLock<TValue>(
        string key,
        TimeSpan expiration,
        Func<Task<TValue?>> dataFactory,
        CancellationToken cancellationToken)
        where TValue : class
    {
        return dataFactory();
    }
}

