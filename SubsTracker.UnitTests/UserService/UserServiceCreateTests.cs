namespace SubsTracker.UnitTests.UserService;

public class UserServiceCreateTests : UserServiceTestsBase
{
    [Fact]
    public async Task Create_WhenCalled_ReturnsCreatedUserDto()
    {
        // Arrange
        var createDto = _fixture.Create<CreateUserDto>();

        var userEntity = _fixture.Build<User>()
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

        _mapper.Map<User>(createDto).Returns(userEntity);
        _repository.Create(userEntity, default).Returns(userEntity);
        _mapper.Map<UserDto>(userEntity).Returns(userDto);

        // Act
        var result = await _service.Create(createDto, default);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(userEntity.Id);
        result.Email.ShouldBe(userEntity.Email);
        result.FirstName.ShouldBe(userEntity.FirstName);
        result.LastName.ShouldBe(userEntity.LastName);
        await _repository.Received(1).Create(userEntity, default);
    }

    [Fact]
    public async Task Create_WhenEmailAlreadyExists_ReturnsInvalidOperationException()
    {
        //Arrange
        var createDto = _fixture.Create<CreateUserDto>();
        var existingUser = _fixture.Build<User>()
            .With(x => x.Email, createDto.Email)
            .Create();

        _repository.GetByPredicate(Arg.Is<Expression<Func<User, bool>>>(expr => expr.Compile().Invoke(existingUser)), default)
            .Returns(existingUser);

        //Act & Assert
        await Should.ThrowAsync<InvalidOperationException>(async () => await _service.Create(createDto, default));
    }

    [Fact]
    public async Task Create_WhenCreateDtoIsNull_ReturnsNull()
    {
        //Act
        var result = await _service.Create(null, default);

        //Assert
        result.ShouldBeNull();
    }

}
