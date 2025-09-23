namespace SubsTracker.UnitTests.UserService;

public class UserServiceDeleteTests : UserServiceTestsBase
{
    [Fact]
    public async Task Delete_WhenCorrectModel_DeletesUser()
    {
        //Arrange
        var existingUser = _fixture.Create<User>();

        _repository.GetById(existingUser.Id, default)
            .Returns(existingUser);

        _repository.Delete(existingUser, default)
            .Returns(true);

        //Act
        var result = await _service.Delete(existingUser.Id, default);

        //Assert
        result.ShouldBeTrue();
        await _repository.Received(1).GetById(existingUser.Id, default);
        await _repository.Received(1).Delete(existingUser, default);
    }


    [Fact]
    public async Task Delete_WhenEmptyGuid_ThrowsNotFoundException()
    {
        //Arrange
        var emptyGuid = Guid.Empty;

        _repository.GetById(emptyGuid, default).Returns((User)null);
        
        //Act & Assert
        await Should.ThrowAsync<NotFoundException>(async () => await _service.Delete(emptyGuid, default));
    }
}

