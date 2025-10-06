namespace SubsTracker.IntegrationTests.Helpers.UserGroup;

public class UserGroupTestsAssertionHelper(TestsWebApplicationFactory factory) : TestHelperBase(factory) 
{
    private readonly IServiceScope _scope = factory.Services.CreateScope();

    public async Task GetByIdHappyPathAssert(HttpResponseMessage response, Group expected)
    {
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var viewModel = JsonConvert.DeserializeObject<UserGroupViewModel>(content);

        viewModel.ShouldNotBeNull();
        viewModel.Id.ShouldBe(expected.Id);
        viewModel.Name.ShouldBe(expected.Name);
    }

    public async Task GetAllHappyPathAssert(HttpResponseMessage response, string expectedName)
    {
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<List<UserGroupViewModel>>(content);

        result.ShouldNotBeNull();
        result.ShouldContain(g => g.Name == expectedName);
    }

    public async Task GetAllSadPathAssert(HttpResponseMessage response)
    {
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<List<UserGroupViewModel>>(content);

        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
    }

    public async Task CreateHappyPathAssert(HttpResponseMessage response, CreateUserGroupDto expected)
    {
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var viewModel = JsonConvert.DeserializeObject<UserGroupViewModel>(content);

        viewModel.ShouldNotBeNull();
        viewModel.Name.ShouldBe(expected.Name);

        var db = _scope.ServiceProvider.GetRequiredService<SubsDbContext>();
        var entity = await db.UserGroups.FindAsync(viewModel.Id);

        entity.ShouldNotBeNull();
        entity!.Name.ShouldBe(expected.Name);
    }

    public async Task UpdateHappyPathAssert(HttpResponseMessage response, Guid groupId, string expectedName)
    {
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var viewModel = JsonConvert.DeserializeObject<UserGroupViewModel>(content);

        viewModel.ShouldNotBeNull();
        viewModel.Id.ShouldBe(groupId);
        viewModel.Name.ShouldBe(expectedName);

        var db = _scope.ServiceProvider.GetRequiredService<SubsDbContext>();
        var entity = await db.UserGroups.FindAsync(groupId);

        entity.ShouldNotBeNull();
        entity!.Name.ShouldBe(expectedName);
    }

    public async Task DeleteHappyPathAssert(HttpResponseMessage response, Guid groupId)
    {
        response.EnsureSuccessStatusCode();

        var db = _scope.ServiceProvider.GetRequiredService<SubsDbContext>();
        var entity = await db.UserGroups.FindAsync(groupId);

        entity.ShouldBeNull();
    }
    
    public async Task GetAllMembersHappyPathAssert(HttpResponseMessage response, UserGroupSeedEntity seedEntity, MemberRole targetRole)
    {
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var members = JsonConvert.DeserializeObject<List<GroupMemberViewModel>>(content);
        
        members.ShouldNotBeNull();
        members.ShouldAllBe(m => m.Role == targetRole);

        var expected = seedEntity.Members.First(m => m.Role == targetRole);
        var actual = members.FirstOrDefault(m => m.Id == expected.Id);

        actual.ShouldNotBeNull();
        actual.Role.ShouldBe(targetRole);
    }
    
    public async Task JoinGroupHappyPathAssert(HttpResponseMessage response, CreateGroupMemberDto createDto)
    {
        response.EnsureSuccessStatusCode();
        
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SubsDbContext>();

        var member = await db.Members.FirstOrDefaultAsync(m =>
            m.UserId == createDto.UserId && m.GroupId == createDto.GroupId);

        member.ShouldNotBeNull();
        member.Role.ShouldBe(MemberRole.Participant);
    }

    public async Task LeaveGroupHappyPathAssert(HttpResponseMessage response ,Guid groupId, Guid userId)
    {
        response.EnsureSuccessStatusCode();
        
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SubsDbContext>();

        var exists = await db.Members.AnyAsync(m =>
            m.UserId == userId && m.GroupId == groupId);

        exists.ShouldBeFalse();
    }
    
    public async Task ChangeRoleHappyPathAssert(HttpResponseMessage response, MemberRole expectedRole)
    {
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var updated = JsonConvert.DeserializeObject<GroupMemberViewModel>(content);
        
        updated.ShouldNotBeNull();
        updated.Role.ShouldBe(expectedRole);
    }

    public async Task ShareSubscriptionHappyPathAssert(HttpResponseMessage response)
    {
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var updated = JsonConvert.DeserializeObject<UserGroupViewModel>(content);

        updated.ShouldNotBeNull();
    }

    public async Task UnshareSubscriptionHappyPathAssert(HttpResponseMessage response)
    {
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var updated = JsonConvert.DeserializeObject<UserGroupViewModel>(content);

        updated.ShouldNotBeNull();
    }
}
