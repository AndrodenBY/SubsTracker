namespace SubsTracker.UnitTests.UserGroupService;

public class UserGroupUnshareSubscriptionTests : UserGroupServiceTestsBase
{
    [Fact]
    public async Task UnshareSubscription_WhenDataIsValid_RemovesSubscription()
    {
        //Arrange
        var subscription = new Subscription { Id = Guid.NewGuid() };
        var userGroup = _fixture.Build<UserGroup>()
            .With(group => group.SharedSubscriptions, new List<Subscription> { subscription })
            .Create();
        var expectedDto = _fixture.Build<UserGroupDto>()
            .With(group => group.Id, userGroup.Id)
            .With(group => group.Name, userGroup.Name)
            .Create();

        _repository.GetById(userGroup.Id, default)
            .Returns(userGroup);
        _repository.Update(Arg.Any<UserGroup>(), default)
            .Returns(userGroup);

        _mapper.Map<UserGroupDto>(Arg.Any<UserGroup>()).Returns(expectedDto);

        //Act
        var result = await _service.UnshareSubscription(userGroup.Id, subscription.Id, CancellationToken.None);

        //Assert
        result.ShouldNotBeNull();
        await _repository.Received(1).Update(Arg.Is<UserGroup>(g => !g.SharedSubscriptions.Contains(subscription)), default);
    }
    
    [Fact]
    public async Task UnshareSubscription_Should_ThrowNotFoundException_When_GroupDoesNotExist()
    {
        //Arrange
        _repository.GetById(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((UserGroup)null);

        //Act & Assert
        await Should.ThrowAsync<NotFoundException>(async () => await _service.UnshareSubscription(Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None));
    }
}
