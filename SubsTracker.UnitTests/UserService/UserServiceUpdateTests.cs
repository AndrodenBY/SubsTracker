namespace SubsTracker.UnitTests.UserService;

public class UserServiceUpdateTests : UserServiceTestsBase
{
    [Fact]
    public async Task Update_WhenCalled_ReturnsUpdatedUser()
    {
        //Arrange
        var userEntity = Fixture.Create<User>();
        var updateDto = Fixture.Create<UpdateUserDto>();
        var userDto = Fixture.Create<UserDto>();
        
        UserRepository.GetByAuth0Id(userDto.Auth0Id, default)
            .Returns(userEntity);

        UserRepository.Update(Arg.Any<User>(), default).Returns(userEntity);
        Mapper.Map(updateDto, userEntity).Returns(userEntity);
        Mapper.Map<UserDto>(userEntity).Returns(userDto);

        //Act
        var result = await Service.Update(userDto.Auth0Id, updateDto, default);

        //Assert
        result.ShouldNotBeNull();
        await UserRepository.Received(1).Update(Arg.Any<User>(), default);
    }


    [Fact]
    public async Task Update_WhenNull_NotFoundException()
    {
        //Act
        var result = async () => await Service.Update(Guid.Empty, null!, default);

        //Assert
        await result.ShouldThrowAsync<UnknowIdentifierException>();
    }
}
