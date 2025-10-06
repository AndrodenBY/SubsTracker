using SubsTracker.API.ViewModel.User;
using SubsTracker.BLL.DTOs.User.Create;

namespace SubsTracker.IntegrationTests.Helpers.User;

public class UserTestsAssertionHelper(TestsWebApplicationFactory factory) : TestHelperBase(factory)
{
    private readonly IServiceScope _scope = factory.Services.CreateScope();
    public async Task GetByIdHappyPathAssert(HttpResponseMessage response, DAL.Models.User.User expected)
    {
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var viewModel = JsonConvert.DeserializeObject<UserViewModel>(content);

        viewModel.ShouldNotBeNull();
        viewModel.Email.ShouldBe(expected.Email);
        viewModel.FirstName.ShouldBe(expected.FirstName);
        viewModel.LastName.ShouldBe(expected.LastName);
    }

    public async Task GetAllHappyPathAssert(HttpResponseMessage response, string expectedEmail)
    {
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<List<UserViewModel>>(content);

        result.ShouldNotBeNull();
        result.ShouldHaveSingleItem();
        result.Single().Email.ShouldBe(expectedEmail);
    }

    public async Task GetAllSadPathAssert(HttpResponseMessage response)
    {
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<List<UserViewModel>>(content);

        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
    }

    public async Task CreateHappyPathAssert(HttpResponseMessage response, CreateUserDto expected)
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

    public async Task UpdateHappyPathAssert(HttpResponseMessage response, Guid userId, string? expectedFirstName, string? expectedEmail)
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
        entity!.FirstName.ShouldBe(expectedFirstName);
        entity.Email.ShouldBe(expectedEmail);
    }

    public async Task DeleteHappyPathAssert(HttpResponseMessage response, Guid userId)
    {
        response.EnsureSuccessStatusCode();

        var db = _scope.ServiceProvider.GetRequiredService<SubsDbContext>();
        var entity = await db.Users.FindAsync(userId);

        entity.ShouldBeNull();
    }
}
