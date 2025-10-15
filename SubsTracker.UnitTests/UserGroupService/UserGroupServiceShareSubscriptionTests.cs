namespace SubsTracker.UnitTests.UserGroupService;

public class UserGroupServiceShareSubscriptionTests : UserGroupServiceTestsBase
{
    [Fact]
    public async Task ShareSubscription_WhenValidData_AddSubscriptionToGroup()
    {
        //Arrange
        var userGroup = Fixture.Build<UserGroup>()
            .With(group => group.SharedSubscriptions, new List<Subscription>())
            .Create();
        var subscription = new Subscription { Id = Guid.NewGuid(), Price = 9.99m, Content = SubscriptionContent.Design, DueDate = DateOnly.MaxValue, Type = SubscriptionType.Free };
        var expectedDto = Fixture.Build<UserGroupDto>()
            .With(group => group.Id, userGroup.Id)
            .With(group => group.Name, userGroup.Name)
            .Create();

        GroupRepository.GetById(userGroup.Id, default)
           .Returns(userGroup);
        SubscriptionRepository.GetById(subscription.Id, default)
            .Returns(subscription);
        GroupRepository.Update(Arg.Any<UserGroup>(), default)
           .Returns(userGroup);
        Mapper.Map<UserGroupDto>(Arg.Any<UserGroup>()).Returns(expectedDto);

        //Act
        var result = await Service.ShareSubscription(userGroup.Id, subscription.Id, CancellationToken.None);

        //Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(userGroup.Id);
        await GroupRepository.Received(1).Update(Arg.Is<UserGroup>(g => g.SharedSubscriptions.Contains(subscription)), default);
    }

    [Fact]
    public async Task ShareSubscription_WhenGroupDoesNotExist_ThrowsNotFoundException()
    {
        //Arrange
        var nonExistentGroupId = Guid.NewGuid();

        GroupRepository.GetById(nonExistentGroupId, Arg.Any<CancellationToken>())
           .Returns(Task.FromResult<UserGroup?>(null));

        //Act
        var result = async () => await Service.ShareSubscription(nonExistentGroupId, Guid.NewGuid(), default);
        
        //Assert
        await result.ShouldThrowAsync<NotFoundException>();
    }
}
