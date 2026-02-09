using SubsTracker.IntegrationTests.Helpers.User;

namespace SubsTracker.IntegrationTests.User;

public class UsersControllerTests : IClassFixture<TestsWebApplicationFactory>
{
    private readonly UserTestsAssertionHelper _assertHelper;
    private readonly HttpClient _client;
    private readonly UserTestsDataSeedingHelper _dataSeedingHelper;

    public UsersControllerTests(TestsWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _dataSeedingHelper = new UserTestsDataSeedingHelper(factory);
        _assertHelper = new UserTestsAssertionHelper(factory);
    }

    [Fact]
    public async Task GetById_ShouldReturnCorrectUser()
    {
        //Arrange
        var seedData = await _dataSeedingHelper.AddSeedUser();

        //Act
        var response = await _client.GetAsync($"{EndpointConst.User}/{seedData.User.Id}");

        //Assert
        await _assertHelper.GetByIdValidAssert(response, seedData.User);
    }
    
    [Fact]
    public async Task GetByAuth0Id_ShouldReturnCurrentAuthenticatedUser()
    {
        //Arrange
        var seedData = await _dataSeedingHelper.AddSeedUser();

        //Act
        var response = await _client.GetAsync($"{EndpointConst.User}/me");

        //Assert
        await _assertHelper.GetByAuth0IdValidAssert(response, seedData.User);
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
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAll_WhenFilteredByEmail_ReturnsMatchingUser()
    {
        //Arrange
        var seedData = await _dataSeedingHelper.AddSeedUser();

        //Act
        var response = await _client.GetAsync($"{EndpointConst.User}?email={seedData.User.Email}");

        //Assert
        await _assertHelper.GetAllValidAssert(response, seedData.User.Email);
    }

    [Fact]
    public async Task GetAll_WhenFilteredByNonExistentEmail_ReturnsEmptyList()
    {
        //Arrange
        await _dataSeedingHelper.AddSeedUser();

        //Act
        var response = await _client.GetAsync($"{EndpointConst.User}?email=nonexistent@example.com");

        //Assert
        await _assertHelper.GetAllInvalidAssert(response);
    }

    [Fact]
    public async Task Create_WhenValidData_ReturnsCreatedUser()
    {
        //Act
        var createDto = _dataSeedingHelper.AddCreateUserDto();

        //Act
        var response = await _client.PostAsJsonAsync($"{EndpointConst.User}", createDto);

        //Assert
        await _assertHelper.CreateValidAssert(response, createDto);
    }

    [Fact]
    public async Task Update_WhenValidData_ReturnsUpdatedUser()
    {
        //Arrange
        await _dataSeedingHelper.AddSeedUser(); 
        var updateDto = _dataSeedingHelper.AddUpdateUserDto();

        //Act
        var response = await _client.PutAsJsonAsync($"{EndpointConst.User}/me", updateDto);
    
        //Assert
        await _assertHelper.UpdateValidAssert(response, updateDto.FirstName, updateDto.Email);
    }

    [Fact]
    public async Task Delete_WhenAuthenticated_RemovesCurrentUser()
    {
        //Arrange
        var seedData = await _dataSeedingHelper.AddSeedUser(); 
    
        //Act
        var response = await _client.DeleteAsync($"{EndpointConst.User}");

        //Assert
        await _assertHelper.DeleteValidAssert(response, seedData.User.Id);
    }
}
