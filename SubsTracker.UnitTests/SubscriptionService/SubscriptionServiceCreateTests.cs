using SubsTracker.Domain.Exceptions;

namespace SubsTracker.UnitTests.SubscriptionService;

public class SubscriptionServiceCreateTests : SubscriptionServiceTestsBase
{
    [Fact]
    public async Task Create_WhenValidModel_ReturnsCreatedSubscription()
    {
        //Arrange
        var createDto = _fixture.Create<CreateSubscriptionDto>();

        var existingUser = _fixture.Create<User>();

        var subscriptionEntity = _fixture.Build<Subscription>()
            .With(s => s.Name, createDto.Name)
            .With(s => s.Price, createDto.Price)
            .With(s => s.DueDate, createDto.DueDate)
            .With(s => s.Content, createDto.Content)
            .With(s => s.Type, createDto.Type)
            .With(s => s.UserId, existingUser.Id)
            .Create();

        var subscriptionDto = _fixture.Build<SubscriptionDto>()
            .With(s => s.Name, subscriptionEntity.Name)
            .With(s => s.Price, subscriptionEntity.Price)
            .With(s => s.DueDate, subscriptionEntity.DueDate)
            .With(s => s.Content, subscriptionEntity.Content)
            .With(s => s.Type, subscriptionEntity.Type)
            .Create();

        _userRepository.GetById(existingUser.Id, default)
            .Returns(existingUser);
        _mapper.Map<Subscription>(createDto)
            .Returns(subscriptionEntity);
        _repository.Create(subscriptionEntity, default)
            .Returns(subscriptionEntity);
        _mapper.Map<SubscriptionDto>(subscriptionEntity)
            .Returns(subscriptionDto);

        //Act
        var result = await _service.Create(existingUser.Id, createDto, default);

        //Assert
        result.ShouldNotBeNull();
        result.Name.ShouldBe(createDto.Name);
        result.Price.ShouldBe(createDto.Price);
        result.DueDate.ShouldBe(createDto.DueDate);
        result.Content.ShouldBe(createDto.Content);
        result.Type.ShouldBe(createDto.Type);
        await _userRepository.Received(1).GetById(existingUser.Id, default);
        await _repository.Received(1).Create(subscriptionEntity, default);
    }

    [Fact]
    public async Task Create_WhenUserNotExists_ReturnsNull()
    {
        //Arrange
        var createDto = _fixture.Create<CreateSubscriptionDto>();
        
        //Act & Assert
        await Should.ThrowAsync<NotFoundException>(async () => await _service.Create(Guid.NewGuid(), createDto, default));
    }
}
