namespace SubsTracker.UnitTests.UserGroupService;

public class UserGroupServiceDeleteTests : UserGroupServiceTestsBase
{
    [Fact]
    public async Task Delete_WhenCorrectModel_DeletesUserGroup()
    {
        //Arrange
        var userGroupEntity = _fixture.Create<UserGroup>();
        
        _repository.GetById(userGroupEntity.Id, default).Returns(userGroupEntity);
        _repository.Delete(userGroupEntity, default).Returns(true);
        
        //Act
        var result = await _service.Delete(userGroupEntity.Id, default);
        
        //Assert
        result.ShouldBeTrue();
        await _repository.Received(1).Delete(userGroupEntity, default);
    }

    [Fact]
    public async Task Delete_WhenEmptyGuid_ThrowsNotFoundException()
    {
        //Arrange
        var emptyId = Guid.Empty;

        _repository.GetById(emptyId, default).Returns((UserGroup)null);

        //Act & Assert
        await Should.ThrowAsync<NotFoundException>(async () => await _service.Delete(emptyId, default));
    }
}
