namespace SubsTracker.UnitTests.UserService;

public class UserServiceUpdateTests : UserServiceTestsBase
{
    [Fact]
    public async Task Update_WhenNull_ThrowsInvalidOperationException()
    {
        //Act
        var result = async () => await Service.Update(Guid.Empty, null!, default);

        //Assert
        await result.ShouldThrowAsync<InvalidOperationException>();
    }
}
