namespace SubsTracker.UnitTests.UserGroupService;

public class UserGroupServiceShareSubscriptionTests : UserGroupServiceTestsBase
{
    [Fact]
    public async Task ShareSubscription_WhenValidData_AddSubscriptionToGroup()
    {
        //Arrange
        var userGroup = _fixture.Build<UserGroup>()
            .With(group => group.SharedSubscriptions, new List<Subscription>())
            .Create();
        var subscription = new Subscription { Id = Guid.NewGuid(), Type = SubscriptionType.Free, Content = SubscriptionContent.Design, DueDate = DateOnly.MinValue, Price = 9.99m };
        var expectedDto = _fixture.Build<UserGroupDto>()
            .With(group => group.Id, userGroup.Id)
            .With(group => group.Name, userGroup.Name)
            .Create();

        _repository.GetById(userGroup.Id, default)
            .Returns(userGroup);
        _subscriptionRepository.GetById(subscription.Id, default)
            .Returns(subscription);
        _repository.Update(Arg.Any<UserGroup>(), default)
            .Returns(userGroup);
        _mapper.Map<UserGroupDto>(Arg.Any<UserGroup>()).Returns(expectedDto);

        //Act
        var result = await _service.ShareSubscription(userGroup.Id, subscription.Id, CancellationToken.None);

        //Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(userGroup.Id);
        await _repository.Received(1).Update(Arg.Is<UserGroup>(g => g.SharedSubscriptions.Contains(subscription)), default);
    }
    
    [Fact]
    public async Task ShareSubscription_WhenGroupDoesNotExist_ThrowsNotFoundException()
    {
        //Arrange
        var nonExistentGroupId = Guid.NewGuid();
        
        _repository.GetById(nonExistentGroupId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<UserGroup?>(null));

        //Act & Assert
        await Should.ThrowAsync<NotFoundException>(async () => await _service.ShareSubscription(nonExistentGroupId, Guid.NewGuid(), default));;
    }
}
