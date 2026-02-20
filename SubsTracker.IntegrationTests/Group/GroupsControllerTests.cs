using System.Net.Http.Headers;
using System.Net.Http.Json;
using MassTransit.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
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
    private readonly ITestHarness _harness;
    private readonly TestsWebApplicationFactory _factory;

    public GroupsControllerTests(TestsWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", "fake-jwt-token");

        _dataSeedingHelper = new GroupTestsDataSeedingHelper(factory);
        _assertHelper = new GroupTestsAssertionHelper(factory);
        _harness = factory.Services.GetRequiredService<ITestHarness>();
        
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
