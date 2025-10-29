namespace SubsTracker.UnitTests.UserService;

public class UserServiceCreateTests : UserServiceTestsBase
{
    [Fact]
    public async Task Create_WhenCalled_ReturnsCreatedUserDto()
    {
        //Arrange
        var createDto = Fixture.Create<CreateUserDto>();

        var userEntity = Fixture.Build<User>()
            .With(x => x.Email, createDto.Email)
            .With(x => x.FirstName, createDto.FirstName)
            .With(x => x.LastName, createDto.LastName)
            .Create();

        var userDto = new UserDto
        {
            Id = userEntity.Id,
            Email = userEntity.Email,
            FirstName = userEntity.FirstName,
            LastName = userEntity.LastName
        };

        Mapper.Map<User>(createDto).Returns(userEntity);
        UserRepository.Create(userEntity, default).Returns(userEntity);
        Mapper.Map<UserDto>(userEntity).Returns(userDto);

        //Act
        var result = await Service.Create(createDto, default);

        //Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(userEntity.Id);
        result.Email.ShouldBe(userEntity.Email);
        result.FirstName.ShouldBe(userEntity.FirstName);
        result.LastName.ShouldBe(userEntity.LastName);
        await UserRepository.Received(1).Create(userEntity, default);
    }

    [Fact]
    public async Task Create_WhenEmailAlreadyExists_ReturnsInvalidOperationException()
    {
        //Arrange
        var createDto = Fixture.Create<CreateUserDto>();
        var existingUser = Fixture.Build<User>()
            .With(x => x.Email, createDto.Email)
            .Create();

        UserRepository.GetByPredicate(Arg.Is<Expression<Func<User, bool>>>(expr => expr.Compile().Invoke(existingUser)), default)
           .Returns(existingUser);

        //Act
        var result = async () => await Service.Create(createDto, default);
        
        //Assert
        await result.ShouldThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Create_WhenCreateDtoIsNull_ReturnsNull()
    {
        //Act
        var result = await Service.Create(null!, default);

        //Assert
        result.ShouldBeNull();
    }
}

