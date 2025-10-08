namespace SubsTracker.UnitTests.UserGroupService;

public class UserGroupServiceGetByIdTests : UserGroupServiceTestsBase
{
    [Fact]
    public async Task GetById_WhenUserGroupExists_ReturnsUserGroupDto()
    {
        //Arrange
        var userGroupDto = _fixture.Create<UserGroupDto>();
        var userGroup = _fixture.Build<UserGroup>()
            .With(x => x.Id, userGroupDto.Id)
            .With(x => x.Name, userGroupDto.Name)
            .Create();

        _repository.GetById(userGroupDto.Id, default)
            .Returns(userGroup);

        _mapper.Map<UserGroupDto>(userGroup)
            .Returns(userGroupDto);

        //Act
        var result = await _service.GetById(userGroupDto.Id, default);

        //Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(userGroupDto.Id);
        result.Name.ShouldBe(userGroupDto.Name);
    }


    [Fact]
    public async Task GetById_WhenEmptyGuid_ReturnsNull()
    {
        //Arrange
        var emptyId = Guid.Empty;
        
        //Act
        var emptyIdResult = await _service.GetById(emptyId, default);
        
        //Assert
        emptyIdResult.ShouldBeNull();
    }

    [Fact]
    public async Task GetById_WhenUserGroupDoesNotExist_ReturnsNull()
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
        //Arrange
        var userGroupDto = _fixture.Create<UserGroupDto>();
        var userGroup = _fixture.Build<UserGroup>()
            .With(x => x.Id, userGroupDto.Id)
            .With(x => x.Name, userGroupDto.Name)
            .Create();
        
        _repository.GetById(userGroupDto.Id, default)
            .Returns(userGroup);

        _mapper.Map<UserGroupDto>(userGroup)
            .Returns(userGroupDto);
        
        //Act
        await _service.GetById(userGroup.Id, default);
        
        //Assert
        await _repository.Received(1).GetById(userGroup.Id, default);
        _mapper.Received(1).Map<UserGroupDto>(userGroup);
    }
}
