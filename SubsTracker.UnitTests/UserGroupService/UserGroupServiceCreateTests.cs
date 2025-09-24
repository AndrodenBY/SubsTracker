namespace SubsTracker.UnitTests.UserGroupService;

public class UserGroupServiceCreateTests : UserGroupServiceTestsBase
{
    [Fact]
    public async Task Create_WhenCalled_ReturnsCreatedUserGroupDto()
    {
        //Arrange
        var createDto = _fixture.Create<CreateUserGroupDto>();
        var userGroupEntity = _fixture.Build<UserGroup>()
            .With(userGroup => userGroup.Name, createDto.Name)
            .With(userGroup => userGroup.UserId, createDto.UserId)
            .Create();
        var userGroupDto = _fixture.Build<UserGroupDto>()
            .With(userGroup => userGroup.Name, userGroupEntity.Name)
            .With(userGroup => userGroup.Id, userGroupEntity.Id )
            .Create();
        
        _userRepository.GetById(createDto.UserId, default).Returns(new User { Id = createDto.UserId });
        _repository.Create(Arg.Any<UserGroup>(), default).Returns(userGroupEntity);
        _mapper.Map<UserGroup>(Arg.Any<CreateUserGroupDto>()).Returns(userGroupEntity);
        _mapper.Map<UserGroupDto>(Arg.Any<UserGroup>()).Returns(userGroupDto);

        //Act
        var result = await _service.Create(createDto, default);

        //Assert
        result.ShouldNotBeNull();
        await _repository.Received(1).Create(Arg.Any<UserGroup>(), default);
        result.ShouldBeEquivalentTo(userGroupDto);
    }
    
    [Fact]
    public async Task Create_WhenEmptyDto_ThrowsValidationException()
    { 
        //Arrange
        var createDto = new CreateUserGroupDto{ Name = string.Empty, UserId = Guid.Empty};
    
        //Act & Assert
        await Should.ThrowAsync<ValidationException>(async () =>
        {
            await _service.Create(Guid.NewGuid(), createDto, default);
        });
    }
    
    [Fact]
    public async Task Create_WhenCalled_CallsRepositoryExactlyOnce()
    {
        //Arrange
        var createDto = _fixture.Create<CreateUserGroupDto>();
        var userGroupEntity = _fixture.Build<UserGroup>()
            .With(userGroup => userGroup.Name, createDto.Name)
            .With(userGroup => userGroup.UserId, createDto.UserId)
            .Create();
        var userGroupDto = _fixture.Build<UserGroupDto>()
            .With(userGroup => userGroup.Name, userGroupEntity.Name)
            .With(userGroup => userGroup.Id, userGroupEntity.Id )
            .Create();
        
        _userRepository.GetById(createDto.UserId, default)
            .Returns(new User { Id = createDto.UserId });
        _repository.Create(Arg.Any<UserGroup>(), default)
            .Returns(userGroupEntity);
        _mapper.Map<UserGroup>(Arg.Any<CreateUserGroupDto>())
            .Returns(userGroupEntity);
        _mapper.Map<UserGroupDto>(Arg.Any<UserGroup>())
            .Returns(userGroupDto);

        //Act
        await _service.Create(createDto, default);

        //Assert
        await _repository.Received(1).Create(Arg.Any<UserGroup>(), default);
    }
}
