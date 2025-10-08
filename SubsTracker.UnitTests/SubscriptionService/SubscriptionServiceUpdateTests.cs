namespace SubsTracker.UnitTests.SubscriptionService;

public class SubscriptionServiceUpdateTests : SubscriptionServiceTestsBase
{
    [Fact]
    public async Task Update_WhenValidModel_ReturnsUpdatedEntity()
    {
        //Arrange
        var existingUser = _fixture.Build<User>()
            .With(user => user.Id, Guid.NewGuid())
            .Create();
        
        var userId = Guid.NewGuid();
        var subscriptionEntity = _fixture.Build<Subscription>()
            .With(s => s.UserId, userId)
            .Create();
        var updateDto = _fixture.Build<UpdateSubscriptionDto>()
            .With(subscription => subscription.Id, subscriptionEntity.Id)
            .Create();
        var subscriptionDto = _fixture.Build<SubscriptionDto>()
            .With(subscription => subscription.Id, updateDto.Id)
            .With(s => s.Name, updateDto.Name)
            .With(s => s.Price, updateDto.Price)
            .With(s => s.DueDate, updateDto.DueDate)
            .With(s => s.Content, updateDto.Content)
            .With(s => s.Type, updateDto.Type)
            .Create();
        
        var user = _fixture.Build<User>()
            .With(user => user.Id, userId)
            .Create();

        _userRepository.GetById(Arg.Any<Guid>(), default)
            .Returns(user);
        _repository.GetById(updateDto.Id, default).Returns(subscriptionEntity);
        _repository.Update(subscriptionEntity, default).Returns(subscriptionEntity);
        _mapper.Map<Subscription>(updateDto).Returns(subscriptionEntity);
        _mapper.Map<SubscriptionDto>(subscriptionEntity).Returns(subscriptionDto);
        
        //Act
        var result = await _service.Update(existingUser.Id, updateDto, default);

        //Assert
        result.ShouldNotBeNull();
        result.Name.ShouldBe(updateDto.Name);
        result.Name.ShouldNotBe(subscriptionEntity.Name);
        result.Id.ShouldBeEquivalentTo(subscriptionEntity.Id);
        await _repository.Received(1).Update(Arg.Any<Subscription>(), default);
    }
    
    [Fact]
    public async Task Update_WhenIncorrectId_ReturnsNotFoundException()
    {
        //Arrange
        var emptyDto = new UpdateSubscriptionDto();

        //Act & Assert
        await Should.ThrowAsync<NotFoundException>(() => _service.Update(Guid.Empty, emptyDto, default));
    }
}
