using SubsTracker.IntegrationTests.Helpers.User;

namespace SubsTracker.IntegrationTests.User;

[Collection("NonParallelTests")]
public class UsersControllerTests : IClassFixture<TestsWebApplicationFactory>
{
    private readonly TestsWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly UserTestsDataSeedingHelper _dataSeedingHelper;
    private readonly UserTestsAssertionHelper _assertHelper;

    public UsersControllerTests(TestsWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
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
    public async Task GetAll_WhenFilteredByEmail_ReturnsMatchingUser()
    {
        //Arrange
        await _dataSeedingHelper.ClearTestDataWithDependencies();
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
        var seedData = await _dataSeedingHelper.AddSeedUser();
        
        //Act
        var response = await _client.GetAsync($"{EndpointConst.User}?email=nonexistent@example.com");
        
        //Assert
        await _assertHelper.GetAllInvalidAssert(response);
    }

    [Fact]
    public async Task Create_WhenValidData_ReturnsCreatedUser()
    {
        //Act
        await _dataSeedingHelper.ClearTestDataWithDependencies();
        var createDto = await _dataSeedingHelper.AddCreateUserDto();
        
        //Act
        var response = await _client.PostAsJsonAsync($"{EndpointConst.User}", createDto);
        
        //Assert
        await _assertHelper.CreateValidAssert(response, createDto);
    }

    [Fact]
    public async Task Update_WhenValidData_ReturnsUpdatedUser()
    {
        //Arrange
        var seedData = await _dataSeedingHelper.AddSeedUser();
        var updateDto = await _dataSeedingHelper.AddUpdateUserDto(seedData.User.Id);
        
        //Act
        var response = await _client.PutAsJsonAsync($"{EndpointConst.User}/{seedData.User.Id}", updateDto);
        
        //Assert
        await _assertHelper.UpdateValidAssert(response, seedData.User.Id, updateDto.FirstName, updateDto.Email);
    }

    [Fact]
    public async Task Delete_WhenValidId_RemovesUser()
    {
        //Arrange
        var seedData = await _dataSeedingHelper.AddSeedUser();
        
        //Act
        var response = await _client.DeleteAsync($"{EndpointConst.User}/{seedData.User.Id}");
        
        //Assert
        await _assertHelper.DeleteValidAssert(response, seedData.User.Id);
    }
}
