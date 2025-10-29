namespace SubsTracker.UnitTests.SubscriptionService;

public class SubscriptionServiceCreateTests : SubscriptionServiceTestsBase
{
    [Fact]
    public async Task Create_WhenValidModel_ReturnsCreatedSubscription()
    {
        //Arrange
        var createDto = Fixture.Create<CreateSubscriptionDto>();

        var existingUser = Fixture.Create<User>();

        var subscriptionEntity = Fixture.Build<Subscription>()
            .With(s => s.Name, createDto.Name)
            .With(s => s.Price, createDto.Price)
            .With(s => s.DueDate, createDto.DueDate)
            .With(s => s.Content, createDto.Content)
            .With(s => s.Type, createDto.Type)
            .With(s => s.UserId, existingUser.Id)
            .Create();

        var subscriptionDto = Fixture.Build<SubscriptionDto>()
            .With(s => s.Name, subscriptionEntity.Name)
            .With(s => s.Price, subscriptionEntity.Price)
            .With(s => s.DueDate, subscriptionEntity.DueDate)
            .With(s => s.Content, subscriptionEntity.Content)
            .With(s => s.Type, subscriptionEntity.Type)
            .Create();

        UserRepository.GetById(existingUser.Id, default)
            .Returns(existingUser);
        Mapper.Map<Subscription>(createDto)
            .Returns(subscriptionEntity);
        SubscriptionRepository.Create(subscriptionEntity, default)
            .Returns(subscriptionEntity);
        Mapper.Map<SubscriptionDto>(subscriptionEntity)
            .Returns(subscriptionDto);

        //Act
        var result = await Service.Create(existingUser.Id, createDto, default);

        //Assert
        result.ShouldNotBeNull();
        result.Name.ShouldBe(createDto.Name);
        result.Price.ShouldBe(createDto.Price);
        result.DueDate.ShouldBe(createDto.DueDate);
        result.Content.ShouldBe(createDto.Content);
        result.Type.ShouldBe(createDto.Type);
        await UserRepository.Received(1).GetById(existingUser.Id, default);
        await SubscriptionRepository.Received(1).Create(subscriptionEntity, default);
    }

    [Fact]
    public async Task Create_WhenUserNotExists_ReturnsNull()
    {
        //Arrange
        var createDto = Fixture.Create<CreateSubscriptionDto>();

        //Act & Assert
        await Should.ThrowAsync<NotFoundException>(async () =>
            await Service.Create(Guid.NewGuid(), createDto, default));
    }
}
