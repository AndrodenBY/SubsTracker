namespace SubsTracker.UnitTests.UserGroupService;

public class UserGroupServiceGetAllTests : UserGroupServiceTestsBase
{
    [Fact]
    public async Task GetAll_WhenFilteredByName_ReturnsCorrectUserGroup()
    {
        //Arrange
        var userGroupToFind = Fixture.Create<UserGroup>();
        var userGroupDto = Fixture.Build<UserGroupDto>()
            .With(userGroup => userGroup.Name, userGroupToFind.Name)
            .With(userGroup => userGroup.Id, userGroupToFind.Id)
            .Create();

        var filter = new UserGroupFilterDto { Name = userGroupToFind.Name };

        Repository.GetAll(Arg.Any<Expression<Func<UserGroup, bool>>>(), default)
           .Returns(new List<UserGroup> { userGroupToFind });
        Mapper.Map<List<UserGroupDto>>(Arg.Any<List<UserGroup>>()).Returns(new List<UserGroupDto> { userGroupDto });

        //Act
        var result = await Service.GetAll(filter, default);

        //Assert
        await Repository.Received(1).GetAll(
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
        var userGroupToFind = Fixture.Create<UserGroup>();
        Fixture.Build<UserGroupDto>()
            .With(userGroup => userGroup.Name, userGroupToFind.Name)
            .With(userGroup => userGroup.Id, userGroupToFind.Id)
            .Create();

        var filter = new UserGroupFilterDto { Name = "Pv$$YbR3aK3rS123" };

        Repository.GetAll(Arg.Any<Expression<Func<UserGroup, bool>>>(), default)
           .Returns(new List<UserGroup>());
        Mapper.Map<List<UserGroupDto>>(Arg.Any<List<UserGroup>>()).Returns(new List<UserGroupDto>());

        //Act
        var result = await Service.GetAll(filter, default);

        //Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetAll_WhenNoUserGroups_ReturnsEmptyList()
    {
        //Arrange
        var filter = new UserGroupFilterDto();

        Repository.GetAll(Arg.Any<Expression<Func<UserGroup, bool>>>(), default)
           .Returns(new List<UserGroup>());
        Mapper.Map<List<UserGroupDto>>(Arg.Any<List<UserGroup>>()).Returns(new List<UserGroupDto>());

        //Act
        var result = await Service.GetAll(filter, default);

        //Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetAll_WhenFilterIsEmpty_ReturnsAllUserGroups()
    {
        //Arrange
        var userGroups = Fixture.CreateMany<UserGroup>(3).ToList();
        var userGroupDtos = Fixture.CreateMany<UserGroupDto>(3).ToList();

        var filter = new UserGroupFilterDto();

        Repository.GetAll(Arg.Any<Expression<Func<UserGroup, bool>>>(), default)
           .Returns(userGroups);
        Mapper.Map<List<UserGroupDto>>(userGroups).Returns(userGroupDtos);

        //Act
        var result = await Service.GetAll(filter, default);

        //Assert
        result.ShouldNotBeNull();
        result.ShouldNotBeEmpty();
        result.Count.ShouldBe(3);
        result.ShouldBe(userGroupDtos);
    }
}
