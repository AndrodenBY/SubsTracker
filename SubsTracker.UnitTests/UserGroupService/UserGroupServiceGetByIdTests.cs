namespace SubsTracker.UnitTests.UserGroupService;

public class UserGroupServiceGetByIdTests : UserGroupServiceTestsBase
{
    private readonly Guid _userGroupId;
    private readonly UserGroup _userGroupEntity;
    private readonly UserGroupDto _userGroupDto;
    
    public UserGroupServiceGetByIdTests()
    {
        _userGroupId = Guid.NewGuid();
        _userGroupEntity = new UserGroup { Id = _userGroupId, Name = "Test UserGroup", };
        _userGroupDto = new UserGroupDto { Id = _userGroupId, Name = "Test UserGroup", };
        
        _repository.GetById(_userGroupId, default)
            .Returns(Task.FromResult<UserGroup?>(_userGroupEntity));
        
        _mapper.Map<UserGroupDto>(_userGroupEntity)
            .Returns(_userGroupDto);
    }

    [Fact]
    public async Task GetById_ShouldReturnUserGroupDto_WhenUserGroupExists()
    {
        //Act
        var result = await _service.GetById(_userGroupId, default);
        
        //Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(_userGroupEntity.Id);
        result.Name.ShouldBe(_userGroupEntity.Name);
    }

    [Fact]
    public async Task GetById_ShouldReturnNull_WhenEmptyGuid()
    {
        //Arrange
        var emptyId = Guid.Empty;
        
        //Act
        var emptyIdResult = await _service.GetById(emptyId, default);
        
        //Assert
        emptyIdResult.ShouldBeNull();
    }

    [Fact]
    public async Task GetById_ShouldReturnNull_WhenUserGroupDoesNotExist()
    {
        //Arrange
        var fakeId = Guid.NewGuid();
        
        //Act
        var fakeIdResult = await _service.GetById(fakeId, default);
        
        //Assert
        fakeIdResult.ShouldBeNull();
    }

    [Fact]
    public async Task GetById_WhenCalled_CallsRepositoryExactlyOnce()
    {
        //Act
        await _service.GetById(_userGroupId, default);
        
        //Assert
        await _repository.Received(1).GetById(_userGroupId, default);
        _mapper.Received(1).Map<UserGroupDto>(_userGroupEntity);
    }
}
