namespace SubsTracker.UnitTests.UserService;

public class UserServiceDeleteTests : UserServiceTestsBase
{
    [Fact]
    public async Task Delete_WhenCorrectModel_DeletesUser()
    {
        //Arrange
        var existingUser = Fixture.Create<User>();

        Repository.GetById(existingUser.Id, default)
           .Returns(existingUser);

        Repository.Delete(existingUser, default)
           .Returns(true);

        //Act
        var result = await Service.Delete(existingUser.Id, default);

        //Assert
        result.ShouldBeTrue();
        await Repository.Received(1).GetById(existingUser.Id, default);
        await Repository.Received(1).Delete(existingUser, default);
    }


    [Fact]
    public async Task Delete_WhenEmptyGuid_ThrowsNotFoundException()
    {
        //Arrange
        var emptyGuid = Guid.Empty;

        Repository.GetById(emptyGuid, default).Returns((User?)null);

        //Act
        var result = async () => await Service.Delete(emptyGuid, default);
        
        //Assert
        await result.ShouldThrowAsync<NotFoundException>();
    }
}

