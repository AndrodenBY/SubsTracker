namespace SubsTracker.UnitTests.SubscriptionService;

public class SubscriptionServiceCancelSubscriptionTests : SubscriptionServiceTestsBase
{
    [Fact]
    public async Task CancelSubscription_WhenValidModel_ReturnsInactiveSubscription()
    {
        //Arrange
        var userId = Guid.NewGuid();
        var subscriptionId = Guid.NewGuid();
        
        var userEntity = _fixture.Create<User>();
        var subscriptionEntity = _fixture.Build<Subscription>()
            .With(s => s.Id, subscriptionId)
            .With(s => s.UserId, userId)
            .With(s => s.Active, true)
            .Create();
        
        var updatedSubscriptionEntity = _fixture.Build<Subscription>()
            .With(s => s.Id, subscriptionId)
            .With(s => s.UserId, userId)
            .With(s => s.Active, false)
            .Create();

        var updatedSubscriptionDto = _fixture.Build<SubscriptionDto>()
            .With(dto => dto.Id, subscriptionId)
            .Create();
        
        _userRepository.GetById(userId, default)
            .Returns(userEntity);
        _repository.GetById(subscriptionId, default)
            .Returns(subscriptionEntity);
        _repository.Update(Arg.Any<Subscription>(), default)
            .Returns(updatedSubscriptionEntity);
        _mapper.Map<SubscriptionDto>(updatedSubscriptionEntity)
            .Returns(updatedSubscriptionDto);
    
        //Act
        var result = await _service.CancelSubscription(userId, subscriptionId, default);
    
        //Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(subscriptionId);
        await _repository.Received(1).Update(Arg.Is<Subscription>(s => s.Id == subscriptionId && s.Active == false), default);
    }

    [Fact]
    public async Task CancelSubscription_WhenIncorrectId_ReturnsNotFoundException()
    {
        //Arrange
        var emptyDto = new UpdateSubscriptionDto();
        
        //Act & Assert
        await Should.ThrowAsync<NotFoundException>(() => _service.Update(Guid.Empty, emptyDto, default));
    }
}
