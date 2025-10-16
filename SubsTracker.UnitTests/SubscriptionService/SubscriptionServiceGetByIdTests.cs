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
    }

    [Fact]
    public async Task GetById_WhenEmptyGuid_ReturnsNull()
    {
        //Arrange
        var emptyId = Guid.Empty;

        //Act
        var emptyIdResult = await Service.GetById(emptyId, default);

        //Assert
        emptyIdResult.ShouldBeNull();
    }

    [Fact]
    public async Task GetById_WhenSubscriptionDoesNotExist_ReturnsNull()
    {
        //Arrange
        var fakeId = Guid.NewGuid();

        //Act
        var fakeIdResult = await Service.GetById(fakeId, default);

        //Assert
        fakeIdResult.ShouldBeNull();
    }
}
