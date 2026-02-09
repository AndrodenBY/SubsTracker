namespace SubsTracker.IntegrationTests.Helpers.User;

public class UserTestsAssertionHelper(TestsWebApplicationFactory factory) : TestHelperBase(factory)
{
    private readonly IServiceScope _scope = factory.Services.CreateScope();

    public async Task GetByIdValidAssert(HttpResponseMessage response, UserModel expected)
    {
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var viewModel = JsonConvert.DeserializeObject<UserViewModel>(content);

        viewModel.ShouldNotBeNull();
        viewModel.Email.ShouldBe(expected.Email);
        viewModel.FirstName.ShouldBe(expected.FirstName);
        viewModel.LastName.ShouldBe(expected.LastName);
    }

    public async Task GetAllValidAssert(HttpResponseMessage response, string expectedEmail)
    {
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<List<UserViewModel>>(content);

        result.ShouldNotBeNull();
        result.ShouldHaveSingleItem();
        result.Single().Email.ShouldBe(expectedEmail);
    }
    
    public async Task GetByAuth0IdValidAssert(HttpResponseMessage response, UserModel expected)
    {
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var viewModel = JsonConvert.DeserializeObject<UserViewModel>(content);
        
        viewModel.ShouldNotBeNull();
        viewModel.Email.ShouldBe(expected.Email);
        viewModel.FirstName.ShouldBe(expected.FirstName);
        viewModel.LastName.ShouldBe(expected.LastName);
        viewModel.Auth0Id.ShouldBe(expected.Auth0Id);
    }

    public async Task GetAllInvalidAssert(HttpResponseMessage response)
    {
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<List<UserViewModel>>(content);

        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
    }

    public async Task CreateValidAssert(HttpResponseMessage response, CreateUserDto expected)
    {
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var viewModel = JsonConvert.DeserializeObject<UserViewModel>(content);

        viewModel.ShouldNotBeNull();
        viewModel.Email.ShouldBe(expected.Email);
        viewModel.FirstName.ShouldBe(expected.FirstName);
        viewModel.LastName.ShouldBe(expected.LastName);

        var db = _scope.ServiceProvider.GetRequiredService<SubsDbContext>();
        var entity = await db.Users.FirstOrDefaultAsync(user => user.Email == viewModel.Email);

        entity.ShouldNotBeNull();
        entity.FirstName.ShouldBe(expected.FirstName);
        entity.LastName.ShouldBe(expected.LastName);
    }

    public async Task UpdateValidAssert(HttpResponseMessage response, Guid userId, string? expectedFirstName,
        string? expectedEmail)
    {
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var viewModel = JsonConvert.DeserializeObject<UserViewModel>(content);

        viewModel.ShouldNotBeNull();
        viewModel.FirstName.ShouldBe(expectedFirstName);
        viewModel.Email.ShouldBe(expectedEmail);

        var db = _scope.ServiceProvider.GetRequiredService<SubsDbContext>();
        var entity = await db.Users.FindAsync(userId);

        entity.ShouldNotBeNull();
        entity.FirstName.ShouldBe(expectedFirstName);
        entity.Email.ShouldBe(expectedEmail);
    }

    public async Task DeleteValidAssert(HttpResponseMessage response, Guid userId)
    {
        response.EnsureSuccessStatusCode();

        var db = _scope.ServiceProvider.GetRequiredService<SubsDbContext>();
        var entity = await db.Users.FindAsync(userId);

        entity.ShouldBeNull();
    }
}
