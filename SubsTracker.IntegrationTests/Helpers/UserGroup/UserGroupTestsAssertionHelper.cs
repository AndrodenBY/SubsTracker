using SubsTracker.Domain.Pagination;
using SubsTracker.IntegrationTests.Configuration.WebApplicationFactory;

namespace SubsTracker.IntegrationTests.Helpers.UserGroup;

public class UserGroupTestsAssertionHelper(TestsWebApplicationFactory factory) : TestHelperBase(factory)
{
    private readonly IServiceScope _scope = factory.Services.CreateScope();

    public async Task GetByIdValidAssert(HttpResponseMessage response, Group expected)
    {
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var viewModel = JsonConvert.DeserializeObject<UserGroupViewModel>(content);

        viewModel.ShouldNotBeNull();
        viewModel.Id.ShouldBe(expected.Id);
        viewModel.Name.ShouldBe(expected.Name);
    }

    public async Task GetAllValidAssert(HttpResponseMessage response, string expectedName)
    {
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        
        var result = JsonConvert.DeserializeObject<PaginatedList<UserGroupViewModel>>(content);

        result.ShouldNotBeNull();
        result.Items.ShouldNotBeNull();
        
        result.Items.ShouldContain(g => g.Name == expectedName);
    }

    public async Task GetAllInvalidAssert(HttpResponseMessage response)
    {
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<PaginatedList<UserGroupViewModel>>(content);

        result.ShouldNotBeNull();
        result.Items.ShouldBeEmpty();
        result.TotalCount.ShouldBe(0);
    }

    public async Task CreateValidAssert(HttpResponseMessage response, CreateUserGroupDto expected)
    {
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"Expected Success but got {response.StatusCode}. Details: {errorContent}");
        }

        var content = await response.Content.ReadAsStringAsync();
        var viewModel = JsonConvert.DeserializeObject<UserGroupViewModel>(content);

        viewModel.ShouldNotBeNull();
        viewModel.Name.ShouldBe(expected.Name);

        var db = _scope.ServiceProvider.GetRequiredService<SubsDbContext>();
        var entity = await db.UserGroups.FindAsync(viewModel.Id);

        entity.ShouldNotBeNull();
        entity.Name.ShouldBe(expected.Name);
    }

    public async Task UpdateValidAssert(HttpResponseMessage response, Guid groupId, string expectedName)
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
        entity.Name.ShouldBe(expectedName);
    }

    public async Task DeleteValidAssert(HttpResponseMessage response, Guid groupId)
    {
        response.EnsureSuccessStatusCode();

        var db = _scope.ServiceProvider.GetRequiredService<SubsDbContext>();
        var entity = await db.UserGroups.FindAsync(groupId);

        entity.ShouldBeNull();
    }

    public async Task GetAllMembersValidAssert(HttpResponseMessage response, UserGroupSeedEntity seedEntity, MemberRole targetRole)
    {
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<PaginatedList<GroupMemberViewModel>>(content);

        result.ShouldNotBeNull();
        result.Items.ShouldNotBeNull();
        result.Items.ShouldAllBe(m => m.Role == targetRole);

        var expected = seedEntity.Members.First(m => m.Role == targetRole);
        var actual = result.Items.FirstOrDefault(m => m.Id == expected.Id);

        actual.ShouldNotBeNull();
        actual.Role.ShouldBe(targetRole);
    }

    public async Task JoinGroupValidAssert(HttpResponseMessage response, CreateGroupMemberDto createDto)
    {
        response.EnsureSuccessStatusCode();

        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SubsDbContext>();

        var member = await db.Members.FirstOrDefaultAsync(m =>
            m.UserId == createDto.UserId && m.GroupId == createDto.GroupId);

        member.ShouldNotBeNull();
        member.Role.ShouldBe(MemberRole.Participant);
    }

    public async Task LeaveGroupValidAssert(HttpResponseMessage response, Guid groupId, Guid userId)
    {
        response.EnsureSuccessStatusCode();

        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SubsDbContext>();

        var exists = await db.Members.AnyAsync(m =>
            m.UserId == userId && m.GroupId == groupId);

        exists.ShouldBeFalse();
    }

    public async Task ChangeRoleValidAssert(HttpResponseMessage response, MemberRole expectedRole)
    {
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var updated = JsonConvert.DeserializeObject<GroupMemberViewModel>(content);

        updated.ShouldNotBeNull();
        updated.Role.ShouldBe(expectedRole);
    }

    public async Task ShareSubscriptionValidAssert(HttpResponseMessage response)
    {
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var updated = JsonConvert.DeserializeObject<UserGroupViewModel>(content);

        updated.ShouldNotBeNull();
    }

    public async Task UnshareSubscriptionValidAssert(HttpResponseMessage response)
    {
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var updated = JsonConvert.DeserializeObject<UserGroupViewModel>(content);

        updated.ShouldNotBeNull();
    }
}
