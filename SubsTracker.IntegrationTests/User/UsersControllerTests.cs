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
        var seedData = await _dataSeedingHelper.AddSeedUser();
        var updateDto = _dataSeedingHelper.AddUpdateUserDto(seedData.User.Id);

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