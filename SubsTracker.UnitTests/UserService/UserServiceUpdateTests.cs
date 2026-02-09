namespace SubsTracker.UnitTests.UserService;

public class UserServiceUpdateTests : UserServiceTestsBase
{
    [Fact]
    public async Task Update_WhenCalled_ReturnsUpdatedUser()
    {
        //Arrange
        var auth0Id = "auth0|test-user-12345";
        var userEntity = Fixture.Build<User>()
            .With(u => u.Auth0Id, auth0Id)
            .Create();
        
        var updateDto = Fixture.Create<UpdateUserDto>();
    
        var userDto = Fixture.Build<UserDto>()
            .With(d => d.Id, userEntity.Id)
            .Create();
        
        UserRepository.GetByAuth0Id(auth0Id, Arg.Any<CancellationToken>()).Returns(userEntity);
        UserRepository.Update(Arg.Any<User>(), Arg.Any<CancellationToken>()).Returns(userEntity);
        
        Mapper.Map(updateDto, userEntity).Returns(userEntity);
        Mapper.Map<UserDto>(userEntity).Returns(userDto);

        //Act
        var result = await Service.Update(auth0Id, updateDto, default);

        //Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(userEntity.Id);
        await UserRepository.Received(1).GetByAuth0Id(auth0Id, Arg.Any<CancellationToken>());
        await UserRepository.Received(1).Update(userEntity, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Update_WhenNull_ThrowsNotFoundException()
    {
        //Act
        var result = async () => await Service.Update(Guid.Empty, null!, default);

        //Assert
        await Should.ThrowAsync<NotFoundException>(result);
    }
}
