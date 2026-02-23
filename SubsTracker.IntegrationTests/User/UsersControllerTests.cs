using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using SubsTracker.API.ViewModel;
using SubsTracker.DAL;
using SubsTracker.IntegrationTests.Configuration;
using SubsTracker.IntegrationTests.Constants;

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
    public async Task GetById_ShouldReturnCorrectUser()
    {
        //Arrange
        var seedData = await _dataSeedingHelper.AddSeedUser();
        var expected = seedData.UserEntity;

        //Act
        var response = await _client.GetAsync($"{EndpointConst.User}/{expected.Id}");

        //Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<UserViewModel>();
        result.ShouldNotBeNull();
        result.ShouldSatisfyAllConditions(
            () => result.Id.ShouldBe(expected.Id),
            () => result.Email.ShouldBe(expected.Email),
            () => result.FirstName.ShouldBe(expected.FirstName),
            () => result.LastName.ShouldBe(expected.LastName)
        );
    }
    
    [Fact]
    public async Task GetByAuth0Id_ShouldReturnCurrentAuthenticatedUser()
    {
        //Arrange
        var seed = await _dataSeedingHelper.AddSeedUser();
        var expected = seed.UserEntity;

        //Act
        var response = await _client.GetAsync($"{EndpointConst.User}/me");

        //Assert
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
    }
    
    [Fact]
    public async Task GetByAuth0Id_WhenNoTokenProvided_ReturnsUnauthorized()
    {
        //Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, $"{EndpointConst.User}/me");
        request.Headers.Add("X-Skip-Auth", "true"); 

        //Act
        var response = await _client.SendAsync(request);

        //Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAll_WhenFilteredByEmail_ReturnsMatchingUser()
    {
        //Arrange
        var seed = await _dataSeedingHelper.AddSeedUser();
        var targetEmail = seed.UserEntity.Email;

        //Act
        var response = await _client.GetAsync($"{EndpointConst.User}?email={targetEmail}");

        //Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<List<UserViewModel>>();

        result.ShouldNotBeNull();
        result.ShouldHaveSingleItem()
            .Email.ShouldBe(targetEmail);
    }

    [Fact]
    public async Task GetAll_WhenFilteredByNonExistentEmail_ReturnsEmptyList()
    {
        //Arrange
        await _dataSeedingHelper.AddSeedUser();
        const string nonExistentEmail = "nonexistent@example.com";

        //Act
        var response = await _client.GetAsync($"{EndpointConst.User}?email={nonExistentEmail}");

        //Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<List<UserViewModel>>();
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task Create_WhenValidData_ReturnsCreatedUser()
    {
        //Arrange
        var dto = _dataSeedingHelper.AddCreateUserDto();

        //Act
        var response = await _client.PostAsJsonAsync(EndpointConst.User, dto);

        //Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<UserViewModel>();
        result.ShouldNotBeNull();
        result.ShouldSatisfyAllConditions(
            () => result.Id.ShouldNotBe(Guid.Empty),
            () => result.Email.ShouldBe(dto.Email),
            () => result.FirstName.ShouldBe(dto.FirstName),
            () => result.LastName.ShouldBe(dto.LastName)
        );

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SubsDbContext>();
        var entity = await db.Users.FindAsync(result.Id);
        entity.ShouldNotBeNull();
        entity.Email.ShouldBe(dto.Email);
        entity.FirstName.ShouldBe(dto.FirstName);
    }
    
    [Fact]
    public async Task Update_WhenValidData_ReturnsUpdatedUser()
    {
        //Arrange
        var seed = await _dataSeedingHelper.AddSeedUser();
        var existingUser = seed.UserEntity;
        var updateDto = _dataSeedingHelper.AddUpdateUserDto(existingUser.Id);

        //Act
        var response = await _client.PutAsJsonAsync($"{EndpointConst.User}/me", updateDto);

        //Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<UserViewModel>();
        result.ShouldNotBeNull();
        result.ShouldSatisfyAllConditions(
            () => result.Id.ShouldBe(existingUser.Id),
            () => result.FirstName.ShouldBe(updateDto.FirstName),
            () => result.LastName.ShouldBe(updateDto.LastName)
        );

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SubsDbContext>();
        var entity = await db.Users.FindAsync(existingUser.Id);
        entity.ShouldNotBeNull();
        entity.FirstName.ShouldBe(updateDto.FirstName);
        entity.LastName.ShouldBe(updateDto.LastName);
    }
    
    [Fact]
    public async Task Delete_WhenValidId_RemovesUser()
    {
        //Arrange
        var seed = await _dataSeedingHelper.AddSeedUser();
        var targetId = seed.UserEntity.Id;

        //Act
        var response = await _client.DeleteAsync(EndpointConst.User);

        //Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SubsDbContext>();
        var entity = await db.Users.FindAsync(targetId);
        entity.ShouldBeNull();
    }
}
