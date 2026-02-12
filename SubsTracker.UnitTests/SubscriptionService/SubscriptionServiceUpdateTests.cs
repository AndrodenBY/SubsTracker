namespace SubsTracker.UnitTests.SubscriptionService;

public class SubscriptionServiceUpdateTests : SubscriptionServiceTestsBase
{
    [Fact]
    public async Task Update_WhenValidModel_ReturnsUpdatedEntity()
    {
        //Arrange
        var existingUser = Fixture.Build<User>()
            .With(user => user.Id, Guid.NewGuid())
            .Create();

        var userId = Guid.NewGuid();
        var subscriptionEntity = Fixture.Build<Subscription>()
            .With(s => s.UserId, userId)
            .Create();
        var updateDto = Fixture.Build<UpdateSubscriptionDto>()
            .With(subscription => subscription.Id, subscriptionEntity.Id)
            .Create();
        var subscriptionDto = Fixture.Build<SubscriptionDto>()
            .With(subscription => subscription.Id, updateDto.Id)
            .With(s => s.Name, updateDto.Name)
            .With(s => s.Price, updateDto.Price)
            .With(s => s.DueDate, updateDto.DueDate)
            .With(s => s.Content, updateDto.Content)
            .With(s => s.Type, updateDto.Type)
            .Create();

        var user = Fixture.Build<User>()
            .With(user => user.Id, userId)
            .Create();

        UserRepository.GetById(Arg.Any<Guid>(), default)
            .Returns(user);
        SubscriptionRepository.GetById(updateDto.Id, default).Returns(subscriptionEntity);
        SubscriptionRepository.Update(subscriptionEntity, default).Returns(subscriptionEntity);
        Mapper.Map<Subscription>(updateDto).Returns(subscriptionEntity);
        Mapper.Map<SubscriptionDto>(subscriptionEntity).Returns(subscriptionDto);

        //Act
        var result = await Service.Update(existingUser.Id, updateDto, default);

        //Assert
        result.ShouldNotBeNull();
        result.Name.ShouldBe(updateDto.Name);
        result.Name.ShouldNotBe(subscriptionEntity.Name);
        result.Id.ShouldBeEquivalentTo(subscriptionEntity.Id);
        await SubscriptionRepository.Received(1).Update(Arg.Any<Subscription>(), default);
    }

    [Fact]
    public async Task Update_WhenIncorrectId_ReturnsNotFoundException()
    {
        //Arrange
        var emptyDto = new UpdateSubscriptionDto();

        //Act
        var result = async () => await Service.Update(Guid.Empty, emptyDto, default);

        //Assert
        await result.ShouldThrowAsync<UnknownIdentifierException>();
    }
}
