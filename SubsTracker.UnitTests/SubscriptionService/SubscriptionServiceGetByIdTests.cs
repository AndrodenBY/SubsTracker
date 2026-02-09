namespace SubsTracker.UnitTests.SubscriptionService;

public class SubscriptionServiceGetByIdTests : SubscriptionServiceTestsBase
{
    [Fact]
    public async Task GetById_WhenSubscriptionExists_ReturnsSubscriptionDto()
    {
        //Arrange
        var subscriptionEntity = Fixture.Create<Subscription>();

        var subscriptionDto = Fixture.Build<SubscriptionDto>()
            .With(subscription => subscription.Id, subscriptionEntity.Id)
            .With(subscription => subscription.Name, subscriptionEntity.Name)
            .With(subscription => subscription.Price, subscriptionEntity.Price)
            .With(subscription => subscription.DueDate, subscriptionEntity.DueDate)
            .Create();

        var cacheKey = $"{subscriptionDto.Id}:{nameof(SubscriptionDto)}";

        CacheService.CacheDataWithLock(
            Arg.Any<string>(),
            Arg.Any<TimeSpan>(),
            Arg.Any<Func<Task<SubscriptionDto>>>(),
            default
        )!.Returns(callInfo =>
        {
            var factory = callInfo.Arg<Func<Task<SubscriptionDto>>>();
            return factory();
        });
        SubscriptionRepository.GetUserInfoById(subscriptionEntity.Id, default)
            .Returns(subscriptionEntity);

        Mapper.Map<SubscriptionDto>(subscriptionEntity)
            .Returns(subscriptionDto);

        //Act
        var result = await Service.GetUserInfoById(subscriptionEntity.Id, default);

        //Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(subscriptionEntity.Id);
        result.Name.ShouldBe(subscriptionEntity.Name);
        result.Price.ShouldBe(subscriptionEntity.Price);
        result.DueDate.ShouldBe(subscriptionEntity.DueDate);

        await SubscriptionRepository.Received(1).GetUserInfoById(subscriptionEntity.Id, default);
        await CacheService.Received(1).CacheDataWithLock(
            cacheKey,
            Arg.Any<TimeSpan>(),
            Arg.Any<Func<Task<SubscriptionDto>>>(),
            default
        );
    }

    [Fact]
    public async Task GetById_WhenEmptyGuid_ThrowsNotFoundException()
    {
        //Arrange
        var emptyId = Guid.Empty;

        //Act
        var emptyIdResult = async() => await Service.GetById(emptyId, default);

        //Assert
        await Should.ThrowAsync<NotFoundException>(emptyIdResult);
    }

    [Fact]
    public async Task GetById_WhenSubscriptionDoesNotExist_ThrowsNotFoundException()
    {
        //Arrange
        var fakeId = Guid.NewGuid();

        //Act
        var fakeIdResult = async () => await Service.GetById(fakeId, default);

        //Assert
        await Should.ThrowAsync<NotFoundException>(fakeIdResult);
    }

    [Fact]
    public async Task GetById_WhenCacheHit_ReturnsCachedDataAndSkipsRepo()
    {
        //Arrange
        var cachedDto = Fixture.Create<SubscriptionDto>();
        var cacheKey = RedisKeySetter.SetCacheKey<SubscriptionDto>(cachedDto.Id);

        CacheService.CacheDataWithLock(
            cacheKey,
            Arg.Any<TimeSpan>(),
            Arg.Any<Func<Task<SubscriptionDto>>>(),
            default
        ).Returns(cachedDto);

        //Act
        var result = await Service.GetUserInfoById(cachedDto.Id, default);

        //Assert
        result.ShouldBe(cachedDto);

        await SubscriptionRepository.DidNotReceive().GetUserInfoById(Arg.Any<Guid>(), default);
        await CacheService.Received(1).CacheDataWithLock(
            cacheKey,
            Arg.Any<TimeSpan>(),
            Arg.Any<Func<Task<SubscriptionDto>>>(),
            default
        );
    }
}
