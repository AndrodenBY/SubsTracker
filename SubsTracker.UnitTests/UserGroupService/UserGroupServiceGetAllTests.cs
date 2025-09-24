namespace SubsTracker.UnitTests.UserGroupService;

public class UserGroupServiceGetAllTests : UserGroupServiceTestsBase
{
    [Fact]
    public async Task GetAll_WhenFilteredByName_ReturnsCorrectUserGroup()
    {
        //Arrange
        var userGroupToFind = _fixture.Create<UserGroup>();
        var userGroupDto = _fixture.Build<UserGroupDto>() 
            .With(userGroup => userGroup.Name, userGroupToFind.Name)
            .With(userGroup => userGroup.Id, userGroupToFind.Id)
            .Create();
        
        var filter = new UserGroupFilterDto { Name = userGroupToFind.Name };

        _repository.GetAll(Arg.Any<Expression<Func<UserGroup, bool>>>(), default)
            .Returns(new List<UserGroup> { userGroupToFind });
        _mapper.Map<List<UserGroupDto>>(Arg.Any<List<UserGroup>>()).Returns(new List<UserGroupDto> { userGroupDto });

        //Act
        var result = await _service.GetAll(filter, default);
        
        //Assert
        await _repository.Received(1).GetAll(
            Arg.Any<Expression<Func<UserGroup, bool>>>(), 
            default
        );
        
        result.ShouldNotBeNull();
        result.ShouldHaveSingleItem();
        result.Single().Name.ShouldBe(userGroupToFind.Name);
    }

    [Fact]
    public async Task GetAll_WhenFilteredByNonExistentName_ReturnsEmptyList()
    {
        //Arrange
        var userGroupToFind = _fixture.Create<UserGroup>();
        var userGroupDto = _fixture.Build<UserGroupDto>()
            .With(userGroup => userGroup.Name, userGroupToFind.Name)
            .With(userGroup => userGroup.Id, userGroupToFind.Id)
            .Create();
        
        var filter = new UserGroupFilterDto { Name = "Pv$$YbR3aK3rS123" };

        _repository.GetAll(Arg.Any<Expression<Func<UserGroup, bool>>>(), default)
            .Returns(new List<UserGroup>());
        _mapper.Map<List<UserGroupDto>>(Arg.Any<List<UserGroup>>()).Returns(new List<UserGroupDto>());
        
        //Act
        var result = await _service.GetAll(filter, default);
        
        //Assert
        result.ShouldBeEmpty();   
    }

    [Fact]
    public async Task GetAll_WhenNoUserGroups_ReturnsEmptyList()
    {
        //Arrange
        var filter = new UserGroupFilterDto();
        
        _repository.GetAll(Arg.Any<Expression<Func<UserGroup, bool>>>(), default)
            .Returns(new List<UserGroup>());
        _mapper.Map<List<UserGroupDto>>(Arg.Any<List<UserGroup>>()).Returns(new List<UserGroupDto>());
        
        //Act
        var result = await _service.GetAll(filter, default);
        
        //Assert
        result.ShouldBeEmpty();
    }
    
    [Fact]
    public async Task GetAll_WhenFilterIsEmpty_ReturnsAllUserGroups()
    {
        //Arrange
        var userGroups = _fixture.CreateMany<UserGroup>(3).ToList();
        var userGroupDtos = _fixture.CreateMany<UserGroupDto>(3).ToList();
        
        var filter = new UserGroupFilterDto();

        _repository.GetAll(Arg.Any<Expression<Func<UserGroup, bool>>>(), default)
            .Returns(userGroups);
        _mapper.Map<List<UserGroupDto>>(userGroups).Returns(userGroupDtos);

        //Act
        var result = await _service.GetAll(filter, default);

        //Assert
        result.ShouldNotBeNull();
        result.ShouldNotBeEmpty();
        result.Count.ShouldBe(3);
        result.ShouldBe(userGroupDtos);
    }
}
