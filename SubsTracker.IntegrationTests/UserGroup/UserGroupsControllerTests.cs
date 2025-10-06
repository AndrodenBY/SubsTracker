namespace SubsTracker.IntegrationTests.UserGroup;

public class UserGroupsControllerTests : IClassFixture<TestsWebApplicationFactory>
{
    private readonly TestsWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly UserGroupTestsDataSeedingHelper _dataSeedingHelper;
    private readonly UserGroupTestsAssertionHelper _assertHelper;

    public UserGroupsControllerTests(TestsWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
        _dataSeedingHelper = new UserGroupTestsDataSeedingHelper(factory);
        _assertHelper = new UserGroupTestsAssertionHelper(factory);
    }
    
    [Fact]
    public async Task GetById_WhenValid_ReturnsCorrectGroup()
    {
        //Arrange
        var seedData = await _dataSeedingHelper.AddOnlyUserGroup();

        //Act
        var response = await _client.GetAsync($"{EndpointConst.Group}/{seedData.Group.Id}");

        //Assert
        await _assertHelper.GetByIdHappyPathAssert(response, seedData.Group);
    }

    [Fact]
    public async Task GetAll_WhenFilteredByName_ReturnsMatchingGroup()
    {
        //Arrange
        await _dataSeedingHelper.ClearTestDataWithDependencies();
        var seedData = await _dataSeedingHelper.AddOnlyUserGroup();

        //Act
        var response = await _client.GetAsync($"{EndpointConst.Group}?name={seedData.Group.Name}");

        //Assert
        await _assertHelper.GetAllHappyPathAssert(response, seedData.Group.Name);
    }

    [Fact]
    public async Task GetAll_WhenFilteredByNonExistentName_ReturnsEmptyList()
    {
        //Arrange
        var seedData = await _dataSeedingHelper.AddOnlyUserGroup();

        //Act
        var response = await _client.GetAsync($"{EndpointConst.Group}?name=NonExistent");

        //Assert
        await _assertHelper.GetAllSadPathAssert(response);
    }

    [Fact]
    public async Task Create_WhenValidData_ReturnsCreatedGroup()
    {
        //Arrange
        await _dataSeedingHelper.ClearTestDataWithDependencies();
        var createDto = await _dataSeedingHelper.AddCreateUserGroupDto();
        var seedUser = await _dataSeedingHelper.AddSeedUserOnly();

        //Act
        var response = await _client.PostAsJsonAsync($"{EndpointConst.Group}/{seedUser.User.Id}/create", createDto);

        //Assert
        await _assertHelper.CreateHappyPathAssert(response, createDto);
    }

    [Fact]
    public async Task Update_WhenValidData_ReturnsUpdatedGroup()
    {
        //Arrange
        var seedData = await _dataSeedingHelper.AddOnlyUserGroup();
        var updateDto = await _dataSeedingHelper.AddUpdateUserGroupDto(seedData.Group.Id);

        //Act
        var response = await _client.PutAsJsonAsync($"{EndpointConst.Group}/{seedData.Group.Id}", updateDto);

        //Assert
        await _assertHelper.UpdateHappyPathAssert(response, seedData.Group.Id, "Updated Group Name");
    }

    [Fact]
    public async Task Delete_WhenValidId_RemovesGroup()
    {
        //Arrange
        var seedData = await _dataSeedingHelper.AddOnlyUserGroup();

        //Act
        var response = await _client.DeleteAsync($"{EndpointConst.Group}/{seedData.Group.Id}");

        //Assert
        await _assertHelper.DeleteHappyPathAssert(response, seedData.Group.Id);
    }
    
    [Fact]
    public async Task GetAllMembers_WhenFilteredByRole_ReturnsCorrectMember()
    {
        //Arrange
        var seedData = await _dataSeedingHelper.AddUserGroupWithMembers();
        var targetRole = MemberRole.Admin;

        //Act
        var response = await _client.GetAsync($"{EndpointConst.Group}/members?role={targetRole}");

        //Assert
        await _assertHelper.GetAllMembersHappyPathAssert(response, seedData, targetRole);
    }
    
    [Fact]
    public async Task JoinGroup_WhenValidData_AddsMember()
    {
        //Arrange
        var seedData = await _dataSeedingHelper.AddUserGroupAndUser();
        var createDto = await _dataSeedingHelper.AddCreateGroupMemberDto(seedData.Group.Id);

        //Act
        var response = await _client.PostAsJsonAsync($"{EndpointConst.Group}/join", createDto);

        //Assert
        await _assertHelper.JoinGroupHappyPathAssert(response, createDto);
    }
    
    [Fact]
    public async Task LeaveGroup_WhenValid_RemovesMember()
    {
        //Arrange
        var seedData = await _dataSeedingHelper.AddUserGroupWithMembers();
        var member = seedData.Members.FirstOrDefault();

        //Act
        var response = await _client.DeleteAsync($"{EndpointConst.Group}/leave?groupId={member.GroupId}&userId={member.UserId}");
        
        //Assert
        await _assertHelper.LeaveGroupHappyPathAssert(response, member.GroupId, member.UserId);
    }
    
    [Fact]
    public async Task ChangeRole_WhenValid_UpdatesMemberRole()
    {
        //Arrange
        await _dataSeedingHelper.ClearTestDataWithDependencies();
        var member = await _dataSeedingHelper.AddMemberOnly();

        //Act
        var response = await _client.PatchAsync($"{EndpointConst.Group}/members/{member.Id}/role", null);

        //Assert
        await _assertHelper.ChangeRoleHappyPathAssert(response, MemberRole.Moderator);
    }
    
    [Fact]
    public async Task ShareSubscription_WhenValid_AddsToGroup()
    {
        //Arrange
        var seedData = await _dataSeedingHelper.AddGroupWithSharedSubscription();
        var subscription = await _dataSeedingHelper.AddSubscription();

        //Act
        var response = await _client.PostAsync($"{EndpointConst.Group}/share?groupId={seedData.Group.Id}&subscriptionId={subscription.Id}", null);

        //Assert
        await _assertHelper.ShareSubscriptionHappyPathAssert(response);
    }
    
    [Fact]
    public async Task UnshareSubscription_WhenValid_RemovesFromGroup()
    {
        //Arrange
        var seedData = await _dataSeedingHelper.AddGroupWithSharedSubscription();
        var subscription = seedData.Group.SharedSubscriptions.First();

        //Act
        var response = await _client.PostAsync($"{EndpointConst.Group}/unshare?groupId={seedData.Group.Id}&subscriptionId={subscription.Id}", null);

        //Assert
        await _assertHelper.UnshareSubscriptionHappyPathAssert(response);
    }
}
