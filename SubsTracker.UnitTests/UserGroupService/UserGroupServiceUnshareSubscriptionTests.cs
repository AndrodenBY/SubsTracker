namespace SubsTracker.UnitTests.UserGroupService;

public class UserGroupUnshareSubscriptionTests : UserGroupServiceTestsBase
{
    [Fact]
    public async Task UnshareSubscription_WhenDataIsValid_RemovesSubscription()
    {
        //Arrange
        var subscription = new Subscription
        {
            Id = Guid.NewGuid(), Type = SubscriptionType.Free, Content = SubscriptionContent.Design,
            DueDate = DateOnly.MinValue, Price = 9.99m
        };
        var userGroup = Fixture.Build<UserGroup>()
            .With(group => group.SharedSubscriptions, new List<Subscription> { subscription })
            .Create();
        var expectedDto = Fixture.Build<UserGroupDto>()
            .With(group => group.Id, userGroup.Id)
            .With(group => group.Name, userGroup.Name)
            .Create();

        GroupRepository.GetFullInfoById(userGroup.Id, default)
            .Returns(userGroup);
        GroupRepository.Update(Arg.Any<UserGroup>(), default)
            .Returns(userGroup);

        Mapper.Map<UserGroupDto>(Arg.Any<UserGroup>()).Returns(expectedDto);

        //Act
        var result = await Service.UnshareSubscription(userGroup.Id, subscription.Id, default);

        //Assert
        result.ShouldNotBeNull();
        await GroupRepository.Received(1)
            .Update(Arg.Is<UserGroup>(g => !g.SharedSubscriptions.Contains(subscription)), default);
    }

    [Fact]
    public async Task UnshareSubscription_WhenGroupDoesNotExist_ThrowsNotFoundException()
    {
        //Arrange
        GroupRepository.GetById(Arg.Any<Guid>(), default)
            .Returns((UserGroup?)null);

        //Act
        var result = async () => await Service.UnshareSubscription(Guid.NewGuid(), Guid.NewGuid(), default);

        //Assert
        await result.ShouldThrowAsync<UnknowIdentifierException>();
    }
}
