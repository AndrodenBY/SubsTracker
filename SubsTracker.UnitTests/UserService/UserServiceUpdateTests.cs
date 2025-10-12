namespace SubsTracker.UnitTests.UserService;

public class UserServiceUpdateTests : UserServiceTestsBase
{
    [Fact]
    public async Task Update_WhenCalled_ReturnsUpdatedUser()
    {
        //Arrange
        var userEntity = Fixture.Create<User>();
        var updateDto = Fixture.Build<UpdateUserDto>()
            .With(userGroup => userGroup.Id, userEntity.Id)
            .Create();
        var userDto = Fixture.Build<UserDto>()
            .With(userGroup => userGroup.Id, updateDto.Id)
            .Create();

        Repository.GetById(updateDto.Id, default).Returns(userEntity);
        Repository.Update(Arg.Any<User>(), default).Returns(userEntity);
        Mapper.Map(updateDto, userEntity).Returns(userEntity);
        Mapper.Map<UserDto>(userEntity).Returns(userDto);

        //Act
        var result = await Service.Update(updateDto.Id, updateDto, default);

        //Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBeEquivalentTo(userEntity.Id);
        await Repository.Received(1).Update(Arg.Any<User>(), default);
    }

    [Fact]
    public async Task Update_WhenNull_ThrowsInvalidOperationException()
    {
        //Act
        var result = async () => await Service.Update(Guid.Empty, null!, default);

        //Assert
        await result.ShouldThrowAsync<InvalidOperationException>();
    }
}

