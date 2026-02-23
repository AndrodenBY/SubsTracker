using System.Net.Http.Headers;
using System.Net.Http.Json;
using MassTransit.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using SubsTracker.API.ViewModel;
using SubsTracker.BLL.DTOs.User.Create;
using SubsTracker.DAL;
using SubsTracker.Domain.Enums;
using SubsTracker.IntegrationTests.Configuration;
using SubsTracker.IntegrationTests.Constants;
using SubsTracker.IntegrationTests.Helpers.Group;
using SubsTracker.Messaging.Contracts;

namespace SubsTracker.IntegrationTests.Group;

public class GroupsControllerTests : IClassFixture<TestsWebApplicationFactory>
{
    private readonly GroupTestsAssertionHelper _assertHelper;
    private readonly HttpClient _client;
    private readonly GroupTestsDataSeedingHelper _dataSeedingHelper;
    private readonly TestsWebApplicationFactory _factory;

    public GroupsControllerTests(TestsWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", "fake-jwt-token");

        _dataSeedingHelper = new GroupTestsDataSeedingHelper(factory);
        _assertHelper = new GroupTestsAssertionHelper(factory);
        
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SubsDbContext>();
        dbContext.Database.EnsureDeleted();
        dbContext.Database.EnsureCreated();
    }

    [Fact]
    public async Task GetById_WhenValid_ReturnsCorrectGroup()
    {
        //Arrange
        var seedData = await _dataSeedingHelper.AddOnlyUserGroup();

        //Act
        var response = await _client.GetAsync($"{EndpointConst.Group}/{seedData.GroupEntity.Id}");

        //Assert
        await _assertHelper.GetByIdValidAssert(response, seedData.GroupEntity);
    }
    
    [Fact]
    public async Task ShareSubscription_WhenValid_AddsLinkInDatabase()
    {
        // Arrange
        var groupSeed = await _dataSeedingHelper.AddOnlyUserGroup();
        var subSeed = await _dataSeedingHelper.AddSubscription();

        // Act
        var response = await _client.PostAsync(
            $"{EndpointConst.Group}/share?groupId={groupSeed.GroupEntity.Id}&subscriptionId={subSeed.Id}", null);

        // Assert
        await _assertHelper.ShareSubscriptionValidAssert(response);
        
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SubsDbContext>();
        var groupWithSubs = await db.UserGroups
            .Include(g => g.SharedSubscriptions)
            .FirstOrDefaultAsync(g => g.Id == groupSeed.GroupEntity.Id);

        groupWithSubs!.SharedSubscriptions.ShouldNotBeNull();
        groupWithSubs.SharedSubscriptions.Any(s => s.Id == subSeed.Id).ShouldBeTrue();
    }
    
    [Fact]
    public async Task GetById_ShouldUseCache_OnConsecutiveCalls()
    {
        //Arrange
        var seedData = await _dataSeedingHelper.AddOnlyUserGroup();
        var groupId = seedData.GroupEntity.Id;
        
        //Act
        var response1 = await _client.GetAsync($"{EndpointConst.Group}/{groupId}");
        response1.EnsureSuccessStatusCode();
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SubsDbContext>();
            var group = await db.UserGroups.FindAsync(groupId);
            group!.Name = "ThisNameShouldBeIgnoredByCache";
            await db.SaveChangesAsync();
        }

        //Act
        var response2 = await _client.GetAsync($"{EndpointConst.Group}/{groupId}");
        
        //Assert
        response2.EnsureSuccessStatusCode();
        var content = await response2.Content.ReadAsStringAsync();
        var result = Newtonsoft.Json.JsonConvert.DeserializeObject<GroupViewModel>(content);

        result.ShouldNotBeNull();
        result.Name.ShouldBe(seedData.GroupEntity.Name); 
        result.Name.ShouldNotBe("ThisNameShouldBeIgnoredByCache");
    }
    
    [Fact]
    public async Task ChangeRole_WhenParticipant_PromotesToModerator()
    {
        // Arrange
        var member = await _dataSeedingHelper.AddMemberOnly();
        var harness = _factory.Services.GetRequiredService<ITestHarness>();

        // Act
        var response = await _client.PatchAsync($"{EndpointConst.Group}/members/{member.Id}/role", null);

        // Assert
        await _assertHelper.ChangeRoleValidAssert(response, MemberRole.Moderator);
        
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SubsDbContext>();
        var updatedMember = await db.Members.FindAsync(member.Id);
        updatedMember!.Role.ShouldBe(MemberRole.Moderator);
        var published = await harness.Published.Any<MemberChangedRoleEvent>(); 
        published.ShouldBeTrue();
    }
    
    [Fact]
    public async Task LeaveGroup_WhenValid_RemovesMemberAndPublishesEvent()
    {
        //Arrange
        var seedData = await _dataSeedingHelper.AddUserGroupWithMembers();
        var member = seedData.Members.First(m => m.Role == MemberRole.Participant);
        var harness = _factory.Services.GetRequiredService<ITestHarness>();

        //Act
        var response = await _client.DeleteAsync(
            $"{EndpointConst.Group}/leave?groupId={member.GroupId}&userId={member.UserId}");

        //Assert
        response.EnsureSuccessStatusCode();
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SubsDbContext>();
        var exists = await db.Members.AnyAsync(m => m.Id == member.Id);
        exists.ShouldBeFalse();
        (await harness.Published.Any<MemberLeftGroupEvent>(x => 
            x.Context.Message.Id == member.Id)).ShouldBeTrue();
    }
    
    [Fact]
    public async Task GetAll_WhenFilteredByName_ReturnsMatchingGroup()
    {
        //Arrange
        var seedData = await _dataSeedingHelper.AddOnlyUserGroup();

        //Act
        var response = await _client.GetAsync($"{EndpointConst.Group}?name={seedData.GroupEntity.Name}");

        //Assert
        await _assertHelper.GetAllValidAssert(response, seedData.GroupEntity.Name);
    }

    [Fact]
    public async Task GetAll_WhenFilteredByNonExistentName_ReturnsEmptyList()
    {
        //Arrange
        await _dataSeedingHelper.AddOnlyUserGroup();

        //Act
        var response = await _client.GetAsync($"{EndpointConst.Group}?name=NonExistent");

        //Assert
        await _assertHelper.GetAllInvalidAssert(response);
    }

    [Fact]
    public async Task Update_WhenValidData_ReturnsUpdatedGroup()
    {
        //Arrange
        var seedData = await _dataSeedingHelper.AddOnlyUserGroup();
        var updateDto = _dataSeedingHelper.AddUpdateUserGroupDto(seedData.GroupEntity.Id);

        //Act
        var response = await _client.PutAsJsonAsync($"{EndpointConst.Group}/{seedData.GroupEntity.Id}", updateDto);

        //Assert
        await _assertHelper.UpdateValidAssert(response, seedData.GroupEntity.Id, "Updated Group Name");
    }
    
    [Fact]
    public async Task Create_WhenValidData_CreatesGroupAndAddsOwnerAsAdmin()
    {
        //Arrange
        var seed = await _dataSeedingHelper.AddSeedUserOnly(); 
        var client = _factory.CreateAuthenticatedClient();

        var createDto = new CreateGroupDto 
        { 
            Name = "New Integration Test Group" 
        };
        
        //Act
        var response = await client.PostAsJsonAsync(EndpointConst.Group, createDto);

        //Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<GroupViewModel>();

        result.ShouldNotBeNull();
        result.Name.ShouldBe(createDto.Name);
        
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SubsDbContext>();
    
        var member = await db.Members.FirstOrDefaultAsync(m => 
            m.GroupId == result.Id && m.UserId == seed.UserEntity.Id);

        member.ShouldNotBeNull();
        member.Role.ShouldBe(MemberRole.Admin);
    }
    
    [Fact]
    public async Task Create_WhenUserDoesNotExist_ReturnsBadRequest()
    {
        // Arrange
        var randomAuth0Id = "non-existent-auth0-id";
        var client = _factory.CreateAuthenticatedClient(randomAuth0Id); 

        var createDto = new CreateGroupDto { Name = "Ghost Group" };

        //Act
        var response = await client.PostAsJsonAsync(EndpointConst.Group, createDto);

        //Assert
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.BadRequest);
    
        var error = await response.Content.ReadAsStringAsync();
        error.ShouldContain("does not exist");
    }
    
    [Fact]
    public async Task Create_WhenNameIsEmpty_ReturnsUnprocessableEntity()
    {
        //Arrange
        await _dataSeedingHelper.AddSeedUserOnly();
        var client = _factory.CreateAuthenticatedClient();

        var createDto = new CreateGroupDto { Name = "" };

        //Act
        var response = await client.PostAsJsonAsync(EndpointConst.Group, createDto);

        //Assert
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Delete_WhenValidId_RemovesGroup()
    {
        //Arrange
        var seedData = await _dataSeedingHelper.AddOnlyUserGroup();

        //Act
        var response = await _client.DeleteAsync($"{EndpointConst.Group}/{seedData.GroupEntity.Id}");

        //Assert
        await _assertHelper.DeleteValidAssert(response, seedData.GroupEntity.Id);
    }
}
