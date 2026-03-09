using System.Net;
using System.Net.Http.Json;
using Allure.Net.Commons;
using Allure.Xunit.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using SubsTracker.API.ViewModel;
using SubsTracker.BLL.DTOs.User.Create;
using SubsTracker.BLL.DTOs.User.Update;
using SubsTracker.DAL;
using SubsTracker.DAL.Entities;
using SubsTracker.Domain.Pagination;
using SubsTracker.IntegrationTests.Configuration;
using SubsTracker.IntegrationTests.Constants;
using SubsTracker.IntegrationTests.Helpers;

namespace SubsTracker.IntegrationTests.User;

public class UsersControllerTests : IClassFixture<TestsWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly UserTestsDataSeedingHelper _dataSeedingHelper;
    private readonly TestsWebApplicationFactory _factory;

    public UsersControllerTests(TestsWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _dataSeedingHelper = new UserTestsDataSeedingHelper(factory);
        
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SubsDbContext>();
        dbContext.Database.EnsureDeleted();
        dbContext.Database.EnsureCreated();
    }

    [Fact]
    [AllureSuite("User API")]
    [AllureFeature("Information")]
    [AllureStory("Get User by ID")]
    [AllureDescription("Verifies that the API returns the correct user details when searching by a specific GUID")]
    public async Task GetById_ShouldReturnCorrectUser()
    {
        // Arrange
        UserEntity expected = null!;

        await AllureApi.Step("Arrange: Seed a user in the database", async () => {
            var seedData = await _dataSeedingHelper.AddSeedUser();
            expected = seedData.UserEntity;
        });

        // Act
        HttpResponseMessage response = null!;
        await AllureApi.Step($"Act: GET request for user ID: {expected.Id}", async () => {
            response = await _client.GetAsync($"{EndpointConst.User}/{expected.Id}");
        });

        // Assert
        await AllureApi.Step("Assert: Validate response status and user details", async () => {
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
        
            var result = await response.Content.ReadFromJsonAsync<UserViewModel>();
            result.ShouldNotBeNull();
            result.ShouldSatisfyAllConditions(
                () => result.Id.ShouldBe(expected.Id),
                () => result.Email.ShouldBe(expected.Email),
                () => result.FirstName.ShouldBe(expected.FirstName),
                () => result.LastName.ShouldBe(expected.LastName)
            );
        });
    }
    
    [Fact]
    [AllureSuite("User API")]
    [AllureFeature("Information")]
    [AllureStory("Get My Profile")]
    [AllureDescription("Verifies that /me returns the profile of the currently logged-in Auth0 user")]
    public async Task GetByAuth0Id_ShouldReturnCurrentAuthenticatedUser()
    {
        // Arrange
        UserEntity expected = null!;

        await AllureApi.Step("Arrange: Seed authenticated user", async () => {
            var seed = await _dataSeedingHelper.AddSeedUser();
            expected = seed.UserEntity;
        });

        // Act
        HttpResponseMessage response = null!;
        await AllureApi.Step("Act: GET request to /user/me", async () => {
            response = await _client.GetAsync($"{EndpointConst.User}/me");
        });

        // Assert
        await AllureApi.Step("Assert: Validate profile matches the authenticated identity", async () => {
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
        
            var result = await response.Content.ReadFromJsonAsync<UserViewModel>();
            result.ShouldNotBeNull();
            result.ShouldSatisfyAllConditions(
                () => result.Id.ShouldBe(expected.Id),
                () => result.Auth0Id.ShouldBe(expected.Auth0Id),
                () => result.Email.ShouldBe(expected.Email),
                () => result.FirstName.ShouldBe(expected.FirstName),
                () => result.LastName.ShouldBe(expected.LastName)
            );
        });
    }
    
    [Fact]
    [AllureSuite("User API")]
    [AllureFeature("Security")]
    [AllureStory("Unauthorized Access")]
    [AllureDescription("Ensures the API blocks requests to /me if the authentication token is missing or bypassed")]
    public async Task GetByAuth0Id_WhenNoTokenProvided_ReturnsUnauthorized()
    {
        // Arrange
        HttpRequestMessage request = null!;
        AllureApi.Step("Arrange: Create request with X-Skip-Auth header", () => {
            request = new HttpRequestMessage(HttpMethod.Get, $"{EndpointConst.User}/me");
            request.Headers.Add("X-Skip-Auth", "true"); 
        });

        // Act
        HttpResponseMessage response = null!;
        await AllureApi.Step("Act: Send request without valid token", async () => {
            response = await _client.SendAsync(request);
        });

        // Assert
        AllureApi.Step("Assert: Status code should be 401 Unauthorized", () => {
            response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        });
    }

    [Fact]
    [AllureSuite("User API")]
    [AllureFeature("Filtering")]
    [AllureStory("Filter by Email")]
    [AllureDescription("Verifies that the list of users can be filtered by a specific email address and returns exactly one match")]
    public async Task GetAll_WhenFilteredByEmail_ReturnsMatchingUser()
    {
        // Arrange
        string targetEmail = null!;
        HttpClient client = null!;

        await AllureApi.Step("Arrange: Seed user and prepare authenticated client", async () => {
            var seed = await _dataSeedingHelper.AddSeedUser();
            targetEmail = seed.UserEntity.Email;
            client = _factory.CreateAuthenticatedClient();
        });

        // Act
        HttpResponseMessage response = null!;
        await AllureApi.Step($"Act: GET request with email filter: {targetEmail}", async () => {
            response = await client.GetAsync($"{EndpointConst.User}?email={targetEmail}");
        });

        // Assert
        await AllureApi.Step("Assert: Validate response contains the single matching user", async () => {
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
        
            var result = await response.Content.ReadFromJsonAsync<PaginatedList<UserViewModel>>(TestHelperBase.DefaultJsonOptions);

            result.ShouldNotBeNull();
            result.Items.ShouldHaveSingleItem()
                .Email.ShouldBe(targetEmail);
        });
    }

    [Fact]
    [AllureSuite("User API")]
    [AllureFeature("Filtering")]
    [AllureStory("Filter by Non-Existent Email")]
    [AllureDescription("Verifies that the API returns an empty list and zero total count when no users match the email filter")]
    public async Task GetAll_WhenFilteredByNonExistentEmail_ReturnsEmptyList()
    {
        // Arrange
        const string nonExistentEmail = "nonexistent@example.com";
        HttpClient client = null!;

        await AllureApi.Step("Arrange: Seed a base user and prepare client", async () => {
            await _dataSeedingHelper.AddSeedUser();
            client = _factory.CreateAuthenticatedClient();
        });

        // Act
        HttpResponseMessage response = null!;
        await AllureApi.Step($"Act: GET request for non-existent email: {nonExistentEmail}", async () => {
            response = await client.GetAsync($"{EndpointConst.User}?email={nonExistentEmail}");
        });

        // Assert
        await AllureApi.Step("Assert: Validate response is OK but contains an empty collection", async () => {
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
        
            var result = await response.Content.ReadFromJsonAsync<PaginatedList<UserViewModel>>(TestHelperBase.DefaultJsonOptions);
        
            result.ShouldNotBeNull();
            result.Items.ShouldBeEmpty();
            result.TotalCount.ShouldBe(0);
        });
    }

    [Fact]
    [AllureSuite("User API")]
    [AllureFeature("Lifecycle")]
    [AllureStory("Create User")]
    [AllureDescription("Verifies that valid data creates a user in the system and persists it to the database")]
    public async Task Create_WhenValidData_ReturnsCreatedUser()
    {
        // Arrange
        CreateUserDto dto = null!;
        AllureApi.Step("Arrange: Prepare CreateUserDto", () => {
            dto = _dataSeedingHelper.AddCreateUserDto();
        });

        // Act
        HttpResponseMessage response = null!;
        await AllureApi.Step("Act: POST request to create user", async () => {
            response = await _client.PostAsJsonAsync(EndpointConst.User, dto);
        });

        // Assert
        await AllureApi.Step("Assert: Validate API response matches DTO", async () => {
            response.StatusCode.ShouldBe(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<UserViewModel>();
            result.ShouldNotBeNull();
            result.ShouldSatisfyAllConditions(
                () => result.Id.ShouldNotBe(Guid.Empty),
                () => result.Email.ShouldBe(dto.Email),
                () => result.FirstName.ShouldBe(dto.FirstName),
                () => result.LastName.ShouldBe(dto.LastName)
            );
            
            await AllureApi.Step("Assert: Verify entity exists in Database", async () => {
                using var scope = _factory.Services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<SubsDbContext>();
                var entity = await db.Users.FindAsync(result.Id);
            
                entity.ShouldNotBeNull();
                entity.Email.ShouldBe(dto.Email);
                entity.FirstName.ShouldBe(dto.FirstName);
            });
        });
    }
    
    [Fact]
    [AllureSuite("User API")]
    [AllureFeature("Lifecycle")]
    [AllureStory("Update User")]
    [AllureDescription("Verifies that the authenticated user can update their own profile details")]
    public async Task Update_WhenValidData_ReturnsUpdatedUser()
    {
        // Arrange
        UserEntity existingUser = null!;
        UpdateUserDto updateDto = null!;

        await AllureApi.Step("Arrange: Seed user and prepare update DTO", async () => {
            var seed = await _dataSeedingHelper.AddSeedUser();
            existingUser = seed.UserEntity;
            updateDto = _dataSeedingHelper.AddUpdateUserDto(existingUser.Id);
        });

        // Act
        HttpResponseMessage response = null!;
        await AllureApi.Step("Act: PUT request to update current user profile", async () => {
            response = await _client.PutAsJsonAsync($"{EndpointConst.User}/me", updateDto);
        });

        // Assert
        await AllureApi.Step("Assert: Validate response reflects updated values", async () => {
            response.StatusCode.ShouldBe(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<UserViewModel>();
            result.ShouldNotBeNull();
            result.ShouldSatisfyAllConditions(
                () => result.Id.ShouldBe(existingUser.Id),
                () => result.FirstName.ShouldBe(updateDto.FirstName),
                () => result.LastName.ShouldBe(updateDto.LastName)
            );
        });

        await AllureApi.Step("Assert: Verify database record was updated", async () => {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SubsDbContext>();
            var entity = await db.Users.FindAsync(existingUser.Id);
        
            entity.ShouldNotBeNull();
            entity.FirstName.ShouldBe(updateDto.FirstName);
            entity.LastName.ShouldBe(updateDto.LastName);
        });
    }
    
    [Fact]
    [AllureSuite("User API")]
    [AllureFeature("Lifecycle")]
    [AllureStory("Delete User")]
    [AllureDescription("Verifies that an authenticated user can delete their account and the record is removed from the DB")]
    public async Task Delete_WhenValidId_RemovesUser()
    {
        // Arrange
        var targetId = Guid.Empty;
        await AllureApi.Step("Arrange: Seed user to be deleted", async () => {
            var seed = await _dataSeedingHelper.AddSeedUser();
            targetId = seed.UserEntity.Id;
        });

        // Act
        HttpResponseMessage response = null!;
        await AllureApi.Step("Act: DELETE request for current user", async () => {
            response = await _client.DeleteAsync(EndpointConst.User);
        });

        // Assert
        AllureApi.Step("Assert: API response should be OK", () => {
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
        });

        await AllureApi.Step("Assert: Verify user no longer exists in Database", async () => {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SubsDbContext>();
            var entity = await db.Users.FindAsync(targetId);
            entity.ShouldBeNull();
        });
    }
}
