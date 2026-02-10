using System.Net.Http.Headers;
using MassTransit.Testing;
using SubsTracker.IntegrationTests.Configuration.WebApplicationFactory;
using SubsTracker.Messaging.Contracts;

namespace SubsTracker.IntegrationTests.UserGroup;

public class UserGroupsControllerTests :
    IClassFixture<TestsWebApplicationFactory>,
    IAsyncLifetime
{
    private readonly UserGroupTestsAssertionHelper _assertHelper;
    private readonly HttpClient _client;
    private readonly UserGroupTestsDataSeedingHelper _dataSeedingHelper;
    private readonly ITestHarness _harness;

    public UserGroupsControllerTests(TestsWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", "fake-jwt-token");

        _dataSeedingHelper = new UserGroupTestsDataSeedingHelper(factory);
        _assertHelper = new UserGroupTestsAssertionHelper(factory);
        _harness = factory.Services.GetRequiredService<ITestHarness>();
    }

    public async Task InitializeAsync() => await _harness.Start();
    public async Task DisposeAsync() => await _harness.Stop();

    [Fact]
    public async Task ChangeRole_WhenValid_ShouldPublishMemberChangedRoleEvent()
    {
        var member = await _dataSeedingHelper.AddMemberOnly();
        
        var response = await _client.PatchAsync(
            $"{EndpointConst.Group}/members/{member.Id}/role", null);
        
        await _assertHelper.ChangeRoleValidAssert(response, MemberRole.Moderator);
        
        Assert.True(
            _harness.Published.Select<MemberChangedRoleEvent>().Any(),
            "Expected MemberChangedRoleEvent to be published"
        );
    }

    [Fact]
    public async Task LeaveGroup_WhenValid_ShouldPublishMemberLeftGroupEvent()
    {
        var seedData = await _dataSeedingHelper.AddUserGroupWithMembers();
        var member = seedData.Members.First();
        
        var response = await _client.DeleteAsync(
            $"{EndpointConst.Group}/leave?groupId={member.GroupId}&userId={member.UserId}");
        
        await _assertHelper.LeaveGroupValidAssert(response, member.GroupId, member.UserId);
        
        Assert.True(
            _harness.Published.Select<MemberLeftGroupEvent>().Any(),
            "Expected MemberLeftGroupEvent to be published"
        );
    }
}
