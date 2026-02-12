namespace SubsTracker.UnitTests.CacheService;

public class CacheServiceTestsBase
{
    protected readonly IDistributedCache CacheMock;
    protected readonly IDistributedLockFactory LockFactory;
    private readonly ILogger<CachingService> _logger;
    protected readonly CachingService Service;

    protected CacheServiceTestsBase()
    {
        CacheMock = Substitute.For<IDistributedCache>();
        LockFactory = Substitute.For<IDistributedLockFactory>();
        _logger = Substitute.For<ILogger<CachingService>>();
        
        Service = new CachingService(CacheMock, _logger, LockFactory);
    }

    protected static byte[] ToBytes<T>(T value)
    {
        var json = JsonConvert.SerializeObject(value, NewtonsoftJsonSettings.Default);
        return Encoding.UTF8.GetBytes(json);
    }
}
