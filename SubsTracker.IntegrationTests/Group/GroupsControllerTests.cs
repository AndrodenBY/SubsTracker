using System.Net;
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
using SubsTracker.Messaging.Contracts;

namespace SubsTracker.IntegrationTests.Group;

public class GroupsControllerTests : IClassFixture<TestsWebApplicationFactory>
{
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
        
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SubsDbContext>();
        dbContext.Database.EnsureDeleted();
        dbContext.Database.EnsureCreated();
    }

    [Fact]
    public async Task GetById_WhenValid_ReturnsCorrectGroup()
    {
        //Arrange
        var seed = await _dataSeedingHelper.AddOnlyUserGroup();
        var expected = seed.GroupEntity;

        //Act
        var response = await _client.GetAsync($"{EndpointConst.Group}/{expected.Id}");

        //Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<GroupViewModel>();

        result.ShouldNotBeNull();
        result.ShouldSatisfyAllConditions(
            () => result.Id.ShouldBe(expected.Id),
            () => result.Name.ShouldBe(expected.Name)
        );
    }
    
    [Fact]
    public async Task ShareSubscription_WhenValid_AddsLinkInDatabase()
    {
        //Arrange
        var groupSeed = await _dataSeedingHelper.AddOnlyUserGroup();
        var subSeed = await _dataSeedingHelper.AddSubscription();

        //Act
        var response = await _client.PostAsync($"{EndpointConst.Group}/share?groupId={groupSeed.GroupEntity.Id}&subscriptionId={subSeed.Id}", null);

        //Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<GroupViewModel>();
        result.ShouldNotBeNull();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SubsDbContext>();
        var groupWithSubs = await db.UserGroups
            .Include(g => g.SharedSubscriptions)
            .FirstOrDefaultAsync(g => g.Id == groupSeed.GroupEntity.Id);

        groupWithSubs.ShouldNotBeNull();
        groupWithSubs.SharedSubscriptions!.ShouldContain(s => s.Id == subSeed.Id);
    }
    
    [Fact]
    public async Task GetById_ShouldUseCache_OnConsecutiveCalls()
    {
        //Arrange
        var seed = await _dataSeedingHelper.AddOnlyUserGroup();
        var groupId = seed.GroupEntity.Id;
        var originalName = seed.GroupEntity.Name;

        //Act
        var response1 = await _client.GetAsync($"{EndpointConst.Group}/{groupId}");
        response1.StatusCode.ShouldBe(HttpStatusCode.OK);

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
        response2.StatusCode.ShouldBe(HttpStatusCode.OK);

        var result = await response2.Content.ReadFromJsonAsync<GroupViewModel>();
        result.ShouldNotBeNull();
        result.ShouldSatisfyAllConditions(
            () => result.Name.ShouldBe(originalName),
            () => result.Name.ShouldNotBe("ThisNameShouldBeIgnoredByCache")
        );
    }
    
    [Fact]
    public async Task ChangeRole_WhenParticipant_PromotesToModerator()
    {
        //Arrange
        var member = await _dataSeedingHelper.AddMemberOnly();
        var harness = _factory.Services.GetRequiredService<ITestHarness>();

        //Act
        var response = await _client.PatchAsync($"{EndpointConst.Group}/members/{member.Id}/role", null);

        //Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<MemberViewModel>();
        result.ShouldNotBeNull();
        result.Role.ShouldBe(MemberRole.Moderator);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SubsDbContext>();
        var dbMember = await db.Members.FindAsync(member.Id);
        dbMember.ShouldNotBeNull();
        dbMember.Role.ShouldBe(MemberRole.Moderator);

        var wasPublished = await harness.Published.Any<MemberChangedRoleEvent>(x => x.Context.Message.Id == member.Id);
        wasPublished.ShouldBeTrue("MemberChangedRoleEvent was not published.");
    }
    
    [Fact]
    public async Task LeaveGroup_WhenValid_RemovesMemberAndPublishesEvent()
    {
        //Arrange
        var seedData = await _dataSeedingHelper.AddUserGroupWithMembers();
        var member = seedData.Members.First(m => m.Role == MemberRole.Participant);
        var harness = _factory.Services.GetRequiredService<ITestHarness>();

        //Act
        var response = await _client.DeleteAsync($"{EndpointConst.Group}/leave?groupId={member.GroupId}&userId={member.UserId}");

        //Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SubsDbContext>();
        var exists = await db.Members.AnyAsync(m => m.Id == member.Id);
        exists.ShouldBeFalse();

        var wasPublished = await harness.Published.Any<MemberLeftGroupEvent>(x => x.Context.Message.Id == member.Id);
        wasPublished.ShouldBeTrue("MemberLeftGroupEvent was not published.");
    }
    
    [Fact]
    public async Task GetAll_WhenFilteredByName_ReturnsMatchingGroup()
    {
        //Arrange
        var seed = await _dataSeedingHelper.AddOnlyUserGroup();
        var targetName = seed.GroupEntity.Name;

        //Act
        var response = await _client.GetAsync($"{EndpointConst.Group}?name={targetName}");

        //Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<List<GroupViewModel>>();
        result.ShouldNotBeNull();
        result.ShouldHaveSingleItem()
            .Name.ShouldBe(targetName);
    }

    [Fact]
    public async Task GetAll_WhenFilteredByNonExistentName_ReturnsEmptyList()
    {
        //Arrange
        await _dataSeedingHelper.AddOnlyUserGroup();

        //Act
        var response = await _client.GetAsync($"{EndpointConst.Group}?name=NonExistent");

        //Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<List<GroupViewModel>>();
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task Update_WhenValidData_ReturnsUpdatedGroup()
    {
        //Arrange
        var seed = await _dataSeedingHelper.AddOnlyUserGroup();
        var existingGroup = seed.GroupEntity;
        var updateDto = _dataSeedingHelper.AddUpdateUserGroupDto(existingGroup.Id);

        //Act
        var response = await _client.PutAsJsonAsync($"{EndpointConst.Group}/{existingGroup.Id}", updateDto);

        //Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<GroupViewModel>();
        result.ShouldNotBeNull();
        result.ShouldSatisfyAllConditions(
            () => result.Id.ShouldBe(existingGroup.Id),
            () => result.Name.ShouldBe(updateDto.Name)
        );

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SubsDbContext>();
        var entity = await db.UserGroups.FindAsync(existingGroup.Id);
        entity.ShouldNotBeNull();
        entity.Name.ShouldBe(updateDto.Name);
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
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    
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
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Delete_WhenValidId_RemovesGroup()
    {
        //Arrange
        var seed = await _dataSeedingHelper.AddOnlyUserGroup();
        var targetId = seed.GroupEntity.Id;

        //Act
        var response = await _client.DeleteAsync($"{EndpointConst.Group}/{targetId}");

        //Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SubsDbContext>();
        var entity = await db.UserGroups.FindAsync(targetId);
        entity.ShouldBeNull();
    }
}
