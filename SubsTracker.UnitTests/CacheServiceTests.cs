using Microsoft.Extensions.Caching.Distributed;
using NSubstitute;
using RedLockNet;
using Shouldly;
using SubsTracker.UnitTests.TestsBase;

namespace SubsTracker.UnitTests;

public class CacheServiceTests : CacheServiceTestsBase
{
    [Fact]
    public async Task CacheDataWithLock_WhenDataExistsInCache_ReturnsDataImmediately()
    {
        //Arrange
        var key = "existing_key";
        var cachedData = "some_data";
        var ct = CancellationToken.None;
        
        CacheMock.GetAsync(key, Arg.Any<CancellationToken>())
            .Returns(ToBytes(cachedData));

        //Act
        var result = await Service.CacheDataWithLock(
            key, 
            TimeSpan.FromMinutes(5), 
            () => Task.FromResult<string?>("fallback"),
            ct);

        //Assert
        result.ShouldBe(cachedData);
        _ = LockFactory.DidNotReceiveWithAnyArgs().CreateLockAsync(default!, default, default, default);
    }

    [Fact]
    public async Task CacheDataWithLock_WhenCacheEmpty_AcquiresLockAndPopulates()
    {
        //Arrange
        var key = "empty_key";
        var expiration = TimeSpan.FromMinutes(10);
        var expectedData = "new_data";
        
        CacheMock.GetAsync(key, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<byte[]?>(null));
    
        var mockLock = Substitute.For<IRedLock>();
        mockLock.IsAcquired.Returns(true);
        
        LockFactory.CreateLockAsync(
                Arg.Any<string>(), 
                Arg.Any<TimeSpan>(), 
                Arg.Any<TimeSpan>(), 
                Arg.Any<TimeSpan>(), 
                Arg.Any<CancellationToken>())
            .Returns(mockLock);

        //Act
        var result = await Service.CacheDataWithLock(key, expiration, () => Task.FromResult<string?>(expectedData), Arg.Any<CancellationToken>());

        //Assert
        result.ShouldNotBeNull();
        result.ShouldBe(expectedData);
        await CacheMock.Received(1).SetAsync(
            key, 
            Arg.Any<byte[]>(), 
            Arg.Any<DistributedCacheEntryOptions>(), 
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CacheDataWithLock_WhenLockAcquiredByAnotherThread_WaitAndRetry()
    {
        //Arrange
        var key = "busy_key";
        var expectedData = "data_from_another_thread";
        var cachedBytes = ToBytes(expectedData);
        var ct = CancellationToken.None;
        
        CacheMock.GetAsync(key, ct)
            .Returns(
                Task.FromResult<byte[]?>(null), 
                Task.FromResult<byte[]?>(cachedBytes)
            );
    
        var mockLock = Substitute.For<IRedLock>();
        mockLock.IsAcquired.Returns(false); 

        LockFactory.CreateLockAsync(
                key,
                Arg.Any<TimeSpan>(), 
                Arg.Any<TimeSpan>(), 
                Arg.Any<TimeSpan>(), 
                ct)
            .Returns(Task.FromResult(mockLock));

        //Act
        var result = await Service.CacheDataWithLock(
            key, 
            TimeSpan.FromMinutes(5), 
            () => Task.FromResult<string?>("bad_data"),
            ct);

        //Assert
        result.ShouldBe(expectedData);
        await CacheMock.DidNotReceive().SetAsync(
            Arg.Any<string>(), 
            Arg.Any<byte[]>(), 
            Arg.Any<DistributedCacheEntryOptions>(), 
            ct);
    }
}
