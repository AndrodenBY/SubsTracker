using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Allure.Net.Commons;
using Allure.Xunit.Attributes;
using AutoFixture;
using MassTransit.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using SubsTracker.API.ViewModel;
using SubsTracker.BLL.DTOs.User.Create;
using SubsTracker.BLL.DTOs.User.Update;
using SubsTracker.DAL;
using SubsTracker.DAL.Entities;
using SubsTracker.Domain.Enums;
using SubsTracker.Domain.Pagination;
using SubsTracker.IntegrationTests.Configuration;
using SubsTracker.IntegrationTests.Constants;
using SubsTracker.IntegrationTests.DataSeedEntities;
using SubsTracker.IntegrationTests.Helpers;
using SubsTracker.Messaging.Contracts;

namespace SubsTracker.IntegrationTests.Group;

[AllureSuite("Integration Tests")]
[AllureFeature("Group Management")]
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
    [AllureSuite("Group API")]
    [AllureFeature("GetById")]
    [AllureDescription("Verifies that a group can be retrieved by its unique identifier")]
    public async Task GetById_WhenValid_ReturnsCorrectGroup()
    {
        // Arrange
        GroupEntity expected = null!;

        await AllureApi.Step("Arrange: Seed a group into the database", async () => {
            var seed = await _dataSeedingHelper.AddOnlyUserGroup();
            expected = seed.GroupEntity;
        });

        // Act
        HttpResponseMessage response = null!;
        await AllureApi.Step($"Act: GET request to {EndpointConst.Group}/{expected.Id}", async () => {
            response = await _client.GetAsync($"{EndpointConst.Group}/{expected.Id}");
        });

        // Assert
        AllureApi.Step("Assert: Validate response status and body", () => {
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
        });
        
        await AllureApi.Step("Assert: Validate response body", async () => {
            var result = await response.Content.ReadFromJsonAsync<GroupViewModel>();
            result.ShouldNotBeNull();
            result.Id.ShouldBe(expected.Id);
            result.Name.ShouldBe(expected.Name);
        });
    }
    
    [Fact]
    [AllureSuite("Group API")]
    [AllureFeature("Performance")]
    [AllureStory("Caching")]
    [AllureDescription("Verifies that the API returns cached data even if the underlying database record changes")]
    public async Task GetById_ShouldUseCache_OnConsecutiveCalls()
    {
        // Arrange
        var groupId = Guid.Empty;
        string originalName = null!;

        await AllureApi.Step("Arrange: Seed group and prepare IDs", async () => {
            var seed = await _dataSeedingHelper.AddOnlyUserGroup();
            groupId = seed.GroupEntity.Id;
            originalName = seed.GroupEntity.Name;
        });

        // Act & Assert 1
        await AllureApi.Step("Act: First call to populate cache", async () => {
            var response1 = await _client.GetAsync($"{EndpointConst.Group}/{groupId}");
            response1.StatusCode.ShouldBe(HttpStatusCode.OK);
        });
        
        await AllureApi.Step("Database: Directly update record in DB (bypassing API)", async () => {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SubsDbContext>();
            var group = await db.UserGroups.FindAsync(groupId);
            group!.Name = "ThisNameShouldBeIgnoredByCache";
            await db.SaveChangesAsync();
        });

        // Act 2
        HttpResponseMessage response2 = null!;
        await AllureApi.Step("Act: Second call (should hit cache)", async () => {
            response2 = await _client.GetAsync($"{EndpointConst.Group}/{groupId}");
        });

        // Assert 2
        await AllureApi.Step("Assert: Validate that name remains the cached original", async () => {
            response2.StatusCode.ShouldBe(HttpStatusCode.OK);
            var result = await response2.Content.ReadFromJsonAsync<GroupViewModel>();
        
            result.ShouldNotBeNull();
            result.ShouldSatisfyAllConditions(
                () => result.Name.ShouldBe(originalName),
                () => result.Name.ShouldNotBe("ThisNameShouldBeIgnoredByCache")
            );
        });
    }
    
    [Fact]
    [AllureSuite("Group API")]
    [AllureFeature("Sharing")]
    [AllureStory("Share Subscription with Group")]
    [AllureDescription("Verifies that a user can link an existing subscription to a group")]
    public async Task ShareSubscription_WhenValid_AddsLinkInDatabase()
    {
        // Arrange
        GroupSeedEntity groupSeed = null!;
        SubscriptionEntity subSeed = null!;
        string url = null!;

        await AllureApi.Step("Arrange: Seed Group and Subscription", async () => {
            groupSeed = await _dataSeedingHelper.AddOnlyUserGroup();
            subSeed = await _dataSeedingHelper.AddSubscription();
            url = $"{EndpointConst.Group}/share?groupId={groupSeed.GroupEntity.Id}&subscriptionId={subSeed.Id}";
        });

        // Act
        HttpResponseMessage response = null!;
        await AllureApi.Step($"Act: POST request to share subscription {subSeed.Id}", async () => {
            response = await _client.PostAsync(url, null);
        });

        // Assert
        await AllureApi.Step("Assert: Validate API response and View Model", async () => {
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<GroupViewModel>();
            result.ShouldNotBeNull();
            result.SharedSubscriptions?.ShouldContain(s => s.Id == subSeed.Id);
        });

        await AllureApi.Step("Assert: Verify Database persistence", async () => {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SubsDbContext>();

            var groupWithSubs = await db.UserGroups
                .AsNoTracking()
                .Include(g => g.SharedSubscriptions)
                .FirstOrDefaultAsync(g => g.Id == groupSeed.GroupEntity.Id);

            groupWithSubs.ShouldNotBeNull();
            groupWithSubs.SharedSubscriptions.ShouldNotBeEmpty();
            groupWithSubs.SharedSubscriptions.ShouldContain(s => s.Id == subSeed.Id);
        });
    }
    
    [Fact]
    [AllureSuite("Group API")]
    [AllureFeature("Membership")]
    [AllureStory("Role Management")]
    [AllureDescription("Verifies that a participant can be promoted to a moderator and an event is published")]
    public async Task ChangeRole_WhenParticipant_PromotesToModerator()
    {
        // Arrange
        MemberEntity member = null!;
        HttpClient client = null!;
        ITestHarness harness = null!;

        await AllureApi.Step("Arrange: Seed member and prepare test harness", async () => {
            member = await _dataSeedingHelper.AddMemberOnly();
            client = _factory.CreateAuthenticatedClient();
            harness = _factory.Services.GetRequiredService<ITestHarness>();
        });

        // Act
        HttpResponseMessage response = null!;
        await AllureApi.Step($"Act: PATCH request to promote member {member.Id} to Moderator", async () => {
            response = await client.PatchAsync($"{EndpointConst.Group}/members/{member.Id}/role", null);
        });

        // Assert
        await AllureApi.Step("Assert: Validate API response status and body", async () => {
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<MemberViewModel>(TestHelperBase.DefaultJsonOptions);
            result.ShouldNotBeNull();
            result.Role.ShouldBe(MemberRole.Moderator);
        });

        await AllureApi.Step("Assert: Verify Database role update", async () => {
            var dbMember = await _dataSeedingHelper.FindEntityAsync<MemberEntity>(member.Id);
            dbMember.ShouldNotBeNull();
            dbMember.Role.ShouldBe(MemberRole.Moderator);
        });

        await AllureApi.Step("Assert: Verify MemberChangedRoleEvent was published to MassTransit", async () => {
            var wasPublished = await harness.Published.Any<MemberChangedRoleEvent>(x => x.Context.Message.Id == member.Id);
            wasPublished.ShouldBeTrue("MemberChangedRoleEvent was not published.");
        });
    }
    
    [Fact]
    [AllureSuite("Group API")]
    [AllureFeature("Membership")]
    [AllureStory("Leave Group")]
    [AllureDescription("Verifies that a member can leave a group, their record is removed, and a departure event is fired")]
    public async Task LeaveGroup_WhenValid_RemovesMemberAndPublishesEvent()
    {
        // Arrange
        MemberEntity member = null!;
        ITestHarness harness = null!;

        await AllureApi.Step("Arrange: Seed group with members and get participant info", async () => {
            var seedData = await _dataSeedingHelper.AddUserGroupWithMembers();
            member = seedData.Members.First(m => m.Role == MemberRole.Participant);
            harness = _factory.Services.GetRequiredService<ITestHarness>();
        });

        // Act
        HttpResponseMessage response = null!;
        await AllureApi.Step($"Act: DELETE request for member {member.UserId} to leave group {member.GroupId}", async () => {
            response = await _client.DeleteAsync($"{EndpointConst.Group}/leave?groupId={member.GroupId}&userId={member.UserId}");
        });

        // Assert
        AllureApi.Step("Assert: Validate API response is OK", () => {
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
        });

        await AllureApi.Step("Assert: Verify member record is deleted from Database", async () => {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SubsDbContext>();
            var exists = await db.Members.AnyAsync(m => m.Id == member.Id);
            exists.ShouldBeFalse();
        });

        await AllureApi.Step("Assert: Verify MemberLeftGroupEvent was published to MassTransit", async () => {
            var wasPublished = await harness.Published.Any<MemberLeftGroupEvent>(x => x.Context.Message.Id == member.Id);
            wasPublished.ShouldBeTrue("MemberLeftGroupEvent was not published.");
        });
    }
    
    [Fact]
    [AllureSuite("Group API")]
    [AllureFeature("Filtering")]
    [AllureStory("Filter by Name")]
    [AllureDescription("Verifies that providing a valid name filter returns only the matching group")]
    public async Task GetAll_WhenFilteredByName_ReturnsMatchingGroup()
    {
        // Arrange
        string targetName = null!;
        HttpClient client = null!;

        await AllureApi.Step("Arrange: Seed group and prepare authenticated client", async () => {
            var seed = await _dataSeedingHelper.AddOnlyUserGroup();
            targetName = seed.GroupEntity.Name;
            client = _factory.CreateAuthenticatedClient();
        });

        // Act
        HttpResponseMessage response = null!;
        await AllureApi.Step($"Act: GET request with name filter: {targetName}", async () => {
            response = await client.GetAsync($"{EndpointConst.Group}?name={targetName}");
        });

        // Assert
        await AllureApi.Step("Assert: Validate response status and filtered pagination result", async () => {
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
        
            var result = await response.Content.ReadFromJsonAsync<PaginatedList<GroupViewModel>>(TestHelperBase.DefaultJsonOptions);
        
            result.ShouldNotBeNull();
            result.Items.ShouldHaveSingleItem().Name.ShouldBe(targetName);
        });
    }
    
    [Fact]
    [AllureSuite("Group API")]
    [AllureFeature("Filtering")]
    [AllureStory("Filter by Name")]
    [AllureDescription("Verifies that a non-existent name filter returns an empty paginated list")]
    public async Task GetAll_WhenFilteredByNonExistentName_ReturnsEmptyList()
    {
        // Arrange
        HttpClient client = null!;

        await AllureApi.Step("Arrange: Seed initial data and prepare client", async () => {
            await _dataSeedingHelper.AddOnlyUserGroup();
            client = _factory.CreateAuthenticatedClient();
        });

        // Act
        HttpResponseMessage response = null!;
        await AllureApi.Step("Act: GET request with non-existent name filter", async () => {
            response = await client.GetAsync($"{EndpointConst.Group}?name=NonExistent");
        });

        // Assert
        await AllureApi.Step("Assert: Validate response is OK but list is empty", async () => {
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
        
            var result = await response.Content.ReadFromJsonAsync<PaginatedList<GroupViewModel>>(TestHelperBase.DefaultJsonOptions);
        
            result.ShouldNotBeNull();
            result.Items.ShouldBeEmpty();
            result.TotalCount.ShouldBe(0);
        });
    }

    [Fact]
    [AllureSuite("Group API")]
    [AllureFeature("CRUD")]
    [AllureStory("Update Group")]
    [AllureDescription("Verifies that an existing group can be updated and changes are persisted in the database")]
    public async Task Update_WhenValidData_ReturnsUpdatedGroup()
    {
        // Arrange
        GroupEntity existingGroup = null!;
        UpdateGroupDto updateDto = null!;

        await AllureApi.Step("Arrange: Seed group and generate update data via AutoFixture", async () => {
            var seed = await _dataSeedingHelper.AddOnlyUserGroup();
            existingGroup = seed.GroupEntity;
            updateDto = _dataSeedingHelper.AddUpdateUserGroupDto(existingGroup.Id);
    
            var json = JsonSerializer.Serialize(updateDto);
            AllureApi.AddAttachment("Update DTO", "application/json", Encoding.UTF8.GetBytes(json), ".json");
        });

        // Act
        HttpResponseMessage response = null!;
        await AllureApi.Step($"Act: PUT request to update group {existingGroup.Id}", async () => {
            response = await _client.PutAsJsonAsync($"{EndpointConst.Group}/{existingGroup.Id}", updateDto);
        });

        // Assert
        await AllureApi.Step("Assert: Validate API response and Database persistence", async () => {
            response.StatusCode.ShouldBe(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<GroupViewModel>();
            result.ShouldNotBeNull();
            result.Id.ShouldBe(existingGroup.Id);
            result.Name.ShouldBe(updateDto.Name);

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SubsDbContext>();
            var entity = await db.UserGroups.FindAsync(existingGroup.Id);
    
            entity.ShouldNotBeNull();
            entity.Name.ShouldBe(updateDto.Name);
        });
    }
    
    [Fact]
    [AllureSuite("Group API")]
    [AllureFeature("CRUD")]
    [AllureStory("Create Group")]
    [AllureDescription("Verifies that a new group can be created and the creator is automatically assigned as Admin")]
    public async Task Create_WhenValidData_CreatesGroupAndAddsOwnerAsAdmin()
    {
        // Arrange
        GroupSeedEntity seed = null!;
        HttpClient client = null!;
        CreateGroupDto createDto = null!;
        var fixture = new Fixture();

        await AllureApi.Step("Arrange: Prepare authenticated user and random group data", async () => {
            seed = await _dataSeedingHelper.AddSeedUserOnly(); 
            client = _factory.CreateAuthenticatedClient();
        
            createDto = fixture.Build<CreateGroupDto>()
                .With(x => x.Name, $"Group_{fixture.Create<string>()}")
                .Create();
        
            var json = JsonSerializer.Serialize(createDto);
            AllureApi.AddAttachment("Create DTO", "application/json", Encoding.UTF8.GetBytes(json), ".json");
        });

        // Act
        HttpResponseMessage response = null!;
        await AllureApi.Step("Act: POST request to create a new group", async () => {
            response = await client.PostAsJsonAsync(EndpointConst.Group, createDto);
        });

        // Assert
        await AllureApi.Step("Assert: Validate Group creation and Admin role assignment", async () => {
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
        });
    }
    
    [Fact]
    [AllureSuite("Group API")]
    [AllureFeature("Validation")]
    [AllureStory("Create Group")]
    [AllureDescription("Verifies that the API prevents group creation if the authenticated Auth0 ID does not exist in our system")]
    public async Task Create_WhenUserDoesNotExist_ReturnsBadRequest()
    {
        // Arrange
        var fixture = new Fixture();
        var randomAuth0Id = $"auth0|{fixture.Create<Guid>()}";
        var client = _factory.CreateAuthenticatedClient(randomAuth0Id); 
        var createDto = fixture.Build<CreateGroupDto>().Create();

        // Removed 'await' because there is no async work inside this step
        AllureApi.Step($"Arrange: Setup client with non-existent Auth0 ID: {randomAuth0Id}", () => {
            var json = JsonSerializer.Serialize(createDto);
            AllureApi.AddAttachment("Create DTO", "application/json", Encoding.UTF8.GetBytes(json), ".json");
        });

        // Act
        HttpResponseMessage response = null!;
        await AllureApi.Step("Act: POST request to create group with invalid user context", async () => {
            response = await client.PostAsJsonAsync(EndpointConst.Group, createDto);
        });

        // Assert
        await AllureApi.Step("Assert: Validate 400 BadRequest and error message", async () => {
            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

            var error = await response.Content.ReadAsStringAsync();
            AllureApi.AddAttachment("Error Payload", "text/plain", Encoding.UTF8.GetBytes(error), ".txt");
    
            error.ShouldContain("does not exist");
        });
    }
    
    [Fact]
    [AllureSuite("Group API")]
    [AllureFeature("Validation")]
    [AllureStory("Create Group")]
    [AllureDescription("Verifies that the API returns BadRequest when the group name is empty")]
    public async Task Create_WhenNameIsEmpty_ReturnsUnprocessableEntity()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedClient();
        var createDto = new CreateGroupDto { Name = string.Empty };

        await AllureApi.Step("Arrange: Seed user and prepare DTO with empty name", async () => {
            await _dataSeedingHelper.AddSeedUserOnly();
        });

        // Act
        HttpResponseMessage response = null!;
        await AllureApi.Step("Act: POST request with empty group name", async () => {
            response = await client.PostAsJsonAsync(EndpointConst.Group, createDto);
        });

        // Assert
        await AllureApi.Step("Assert: Validate response status is 400 BadRequest", async () => {
            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    
            var error = await response.Content.ReadAsStringAsync();
            AllureApi.AddAttachment("Validation Errors", "application/json", Encoding.UTF8.GetBytes(error), ".json");
        });
    }

    [Fact]
    [AllureSuite("Group API")]
    [AllureFeature("CRUD")]
    [AllureStory("Delete Group")]
    [AllureDescription("Verifies that a group is successfully removed from the database when a valid ID is provided")]
    public async Task Delete_WhenValidId_RemovesGroup()
    {
        // Arrange
        var targetId = Guid.Empty;

        await AllureApi.Step("Arrange: Seed a group into the database and capture its ID", async () => {
            var seed = await _dataSeedingHelper.AddOnlyUserGroup();
            targetId = seed.GroupEntity.Id;
        });

        // Act
        HttpResponseMessage response = null!;
        await AllureApi.Step($"Act: DELETE request to {EndpointConst.Group}/{targetId}", async () => {
            response = await _client.DeleteAsync($"{EndpointConst.Group}/{targetId}");
        });

        // Assert
        await AllureApi.Step("Assert: Validate response status and verify record deletion in DB", async () => {
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SubsDbContext>();
            var entity = await db.UserGroups.FindAsync(targetId);
        
            entity.ShouldBeNull();
        });
    }
}
