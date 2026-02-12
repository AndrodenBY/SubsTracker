using SubsTracker.BLL.Interfaces.Cache;

namespace SubsTracker.IntegrationTests.Configuration;

public class FakeCacheAccessService : ICacheAccessService
{
    private readonly Dictionary<string, object> _store = new();

    public Task<T?> GetData<T>(string key, CancellationToken cancellationToken)
    {
        return Task.FromResult(_store.TryGetValue(key, out var value) ? (T?)value : default);
    }

    public Task SetData<T>(string key, T value, TimeSpan expiration, CancellationToken cancellationToken)
    {
        _store[key] = value!;
        return Task.CompletedTask;
    }

    public Task RemoveData(List<string> keys, CancellationToken cancellationToken)
    {
        foreach (var key in keys)
            _store.Remove(key);

        return Task.CompletedTask;
    }
}

