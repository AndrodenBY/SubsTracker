using System.Text;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NSubstitute;
using RedLockNet;
using SubsTracker.BLL.Json;
using SubsTracker.BLL.Services.Cache;

namespace SubsTracker.UnitTests.TestsBase;

public class CacheServiceTestsBase
{
    protected readonly IDistributedCache CacheMock;
    protected readonly IDistributedLockFactory LockFactory;
    private readonly ILogger<CacheService> _logger;
    protected readonly CacheService Service;

    protected CacheServiceTestsBase()
    {
        CacheMock = Substitute.For<IDistributedCache>();
        LockFactory = Substitute.For<IDistributedLockFactory>();
        _logger = Substitute.For<ILogger<CacheService>>();
        
        Service = new CacheService(CacheMock, _logger, LockFactory);
    }

    protected static byte[] ToBytes<T>(T value)
    {
        var json = JsonConvert.SerializeObject(value, NewtonsoftJsonSettings.Default);
        return Encoding.UTF8.GetBytes(json);
    }
}
