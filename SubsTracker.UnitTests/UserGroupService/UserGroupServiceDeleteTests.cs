namespace SubsTracker.UnitTests.UserGroupService;

public class UserGroupServiceDeleteTests : UserGroupServiceTestsBase
{
    [Fact]
    public async Task Delete_WhenCorrectModel_DeletesUserGroup()
    {
        //Arrange
        var userGroupEntity = Fixture.Create<UserGroup>();

        Repository.GetById(userGroupEntity.Id, default).Returns(userGroupEntity);
        Repository.Delete(userGroupEntity, default).Returns(true);

        //Act
        var result = await Service.Delete(userGroupEntity.Id, default);

        //Assert
        result.ShouldBeTrue();
        await Repository.Received(1).Delete(userGroupEntity, default);
    }

    [Fact]
    public async Task Delete_WhenEmptyGuid_ThrowsNotFoundException()
    {
        //Arrange
        var emptyId = Guid.Empty;

        Repository.GetById(emptyId, default).Returns((UserGroup?)null);

        //Act
        var result = async () => await Service.Delete(emptyId, default);
        
        //Assert
        await result.ShouldThrowAsync<NotFoundException>();
    }
}
