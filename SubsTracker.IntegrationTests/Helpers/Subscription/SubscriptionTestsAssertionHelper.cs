namespace SubsTracker.IntegrationTests.Helpers.Subscription;

public class SubscriptionTestsAssertionHelper
{
    public async Task GetByIdValidAssert(HttpResponseMessage response, SubscriptionModel expected)
    {
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<SubscriptionViewModel>();
        result.ShouldNotBeNull();

        result.Id.ShouldBe(expected.Id);
        result.Name.ShouldBe(expected.Name);
        result.Price.ShouldBe(expected.Price);
        result.DueDate.ShouldBe(expected.DueDate);
    }

    public async Task GetAllValidAssert(HttpResponseMessage response, string expectedName)
    {
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<List<SubscriptionViewModel>>(content);

        result.ShouldNotBeNull();
        result.ShouldHaveSingleItem();
        result.Single().Name.ShouldBe(expectedName);
    }

    public async Task GetAllInvalidAssert(HttpResponseMessage response)
    {
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<List<SubscriptionViewModel>>(content);

        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
    }

    public async Task CreateValidAssert(HttpResponseMessage response)
    {
        response.EnsureSuccessStatusCode();

        var rawContent = await response.Content.ReadAsStringAsync();
        rawContent.ShouldNotBeNullOrWhiteSpace();

        var viewModel = JsonConvert.DeserializeObject<SubscriptionViewModel>(rawContent);
        viewModel.ShouldNotBeNull();
    }

    public async Task UpdateValidAssert(HttpResponseMessage response, Guid expectedId, string expectedName)
    {
        response.EnsureSuccessStatusCode();

        var rawContent = await response.Content.ReadAsStringAsync();
        rawContent.ShouldNotBeNullOrWhiteSpace();

        var viewModel = JsonConvert.DeserializeObject<SubscriptionViewModel>(rawContent);
        viewModel.ShouldNotBeNull();

        viewModel.Id.ShouldBe(expectedId);
        viewModel.Name.ShouldBe(expectedName);
    }

    public async Task CancelSubscriptionValidAssert(HttpResponseMessage response, SubscriptionModel original)
    {
        response.EnsureSuccessStatusCode();

        var rawContent = await response.Content.ReadAsStringAsync();
        rawContent.ShouldNotBeNullOrWhiteSpace();

        var viewModel = JsonConvert.DeserializeObject<SubscriptionViewModel>(rawContent);
        viewModel.ShouldNotBeNull();
        viewModel.Id.ShouldBe(original.Id);
    }

    public async Task RenewSubscriptionValidAssert(HttpResponseMessage response, SubscriptionModel original, DateOnly expectedDueDate)
    {
        response.EnsureSuccessStatusCode();

        var rawContent = await response.Content.ReadAsStringAsync();
        rawContent.ShouldNotBeNullOrWhiteSpace();

        var viewModel = JsonConvert.DeserializeObject<SubscriptionViewModel>(rawContent);
        viewModel.ShouldNotBeNull();

        viewModel.Id.ShouldBe(original.Id);
        viewModel.DueDate.ShouldBe(expectedDueDate);
    }

    public async Task GetUpcomingBillsValidAssert(HttpResponseMessage response, SubscriptionModel expected)
    {
        response.EnsureSuccessStatusCode();

        var rawContent = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<List<SubscriptionViewModel>>(rawContent);

        result.ShouldNotBeNull();
        result.ShouldNotBeNull();
        result.ShouldContain(x => x.Id == expected.Id);
        result.ShouldAllBe(x => x.DueDate <= DateOnly.FromDateTime(DateTime.Today.AddDays(7)));


    }
}
