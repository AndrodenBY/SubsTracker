namespace SubsTracker.IntegrationTests.Subscription;

public class SubscriptionsControllerTests : IClassFixture<TestsWebApplicationFactory>, IAsyncDisposable
{
    private readonly TestsWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly SubscriptionTestHelper _helper;

    public SubscriptionsControllerTests(TestsWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
        _helper = new SubscriptionTestHelper(factory);
    }
    
    [Fact]
    public async Task GetById_ShouldReturnCorrectSubscription()
    {
        //Arrange
        var dataSeedObject = await _helper.AddSeedData();
        var subscription = dataSeedObject.Subscriptions.FirstOrDefault();

        //Act
        var response = await _client.GetAsync($"{EndpointConst.Subscription}/{subscription.Id}");
        
        //Assert
        await _helper.GetByIdHappyPathAssert(response, subscription);
    }

    [Fact]
    public async Task GetAll_WhenFilteredByName_ReturnsOnlyMatchingSubscription()
    {
        //Arrange
        await _helper.ClearTestDataWithRelations();
        var seedData = await _helper.AddSeedUserWithSubscriptions("Target Subscription", "Unrelated App");
        
        //Act
        var response = await _client.GetAsync($"{EndpointConst.Subscription}?Name=Target Subscription");
        
        //Assert
        await _helper.GetAllHappyPathAssert(response, "Target Subscription");
    }
    
    [Fact]
    public async Task GetAll_WhenFilteredByNonExistentName_ReturnsEmptyList()
    {
        //Arrange
        var seedData = await _helper.AddSeedData();
        var nonExistentName = "NonExistentFilter";

        //Act
        var response = await _client.GetAsync($"{EndpointConst.Subscription}?Name={nonExistentName}");
        
        //Assert
        await _helper.GetAllSadPathAssert(response);
    }
    
    [Fact]
    public async Task Create_WhenValidData_ReturnsCreatedSubscription()
    {
        //Arrange
        var subscriptionDto = await _helper.AddCreateSubscriptionDto();
        var dataSeedObject = await _helper.AddSeedUserOnly();
    
        //Act
        var response = await _client.PostAsJsonAsync($"{EndpointConst.Subscription}/{dataSeedObject.User.Id}", subscriptionDto);
        
        //Assert
        await _helper.CreateHappyPathAssert(response);
    }
    
    [Fact]
    public async Task Update_WhenValidData_ReturnsUpdatedSubscription()
    {
        // Arrange
        // 1. Создаем и сохраняем существующую подписку в БД
        var existingUser = _helper._fixture.Build<User>()
            .Without(u => u.Groups)
            .Create();
        var subscriptionEntity = _helper._fixture.Build<Subscription>()
            .With(s => s.UserId, existingUser.Id)
            .With(s => s.User, existingUser)
            .Create();

        // 2. Добавляем пользователя и подписку в БД
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SubsDbContext>();
            await db.Users.AddAsync(existingUser);
            await db.Subscriptions.AddAsync(subscriptionEntity);
            await db.SaveChangesAsync(default);
        }

        // 3. Создаем DTO для обновления
        var updateDto = new UpdateSubscriptionDto
        {
            Id = subscriptionEntity.Id, 
            Name = "Updated Streaming Service Name",
            Price = 49.99m,
        };

        // Act: PUT /api/subscriptions/{id}
        // Используем PutAsJsonAsync для отправки DTO и явного указания Guid в URL
        var response = await _client.PutAsJsonAsync(
            $"/api/subscriptions/{existingUser.Id}", 
            updateDto);

        // Assert 1: Проверка HTTP-статуса
        response.EnsureSuccessStatusCode(); 
    
        // Assert 2: Чтение и проверка возвращаемой ViewModel
        var rawContent = await response.Content.ReadAsStringAsync();
        var resultViewModel = JsonConvert.DeserializeObject<SubscriptionViewModel>(rawContent);

        resultViewModel.ShouldNotBeNull();
        resultViewModel!.Id.ShouldBe(subscriptionEntity.Id);
        resultViewModel.Name.ShouldBe(updateDto.Name);

        // Assert 3: Проверка целостности данных в базе данных
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SubsDbContext>();
            var savedSubscription = db.Subscriptions.SingleOrDefault(s => s.Id == subscriptionEntity.Id);
        
            savedSubscription.ShouldNotBeNull();
            savedSubscription!.Name.ShouldBe(updateDto.Name);
        }
    }
    
    [Fact]
    public async Task CancelSubscription_WhenValidData_ReturnsCancelledSubscription()
    {
        // Arrange
        var user = _helper._fixture.Build<User>()
            .Without(u => u.Groups)
            .Create();

        var subscription = _helper._fixture.Build<Subscription>()
            .With(s => s.UserId, user.Id)
            .With(s => s.User, user)
            .With(s => s.Active, true)
            .Create();

        using (var scope = _helper.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SubsDbContext>();
            db.Users.Add(user);
            db.Subscriptions.Add(subscription);
            await db.SaveChangesAsync(default);
        }

        // Act: PATCH /api/subscriptions/{subscriptionId}/cancel?userId={userId}
        var response = await _client.PatchAsync(
            $"/api/subscriptions/{subscription.Id}/cancel?userId={user.Id}", null);

        // Assert 1: Проверка статуса
        response.EnsureSuccessStatusCode();

        // Assert 2: Проверка возвращаемой ViewModel
        var rawContent = await response.Content.ReadAsStringAsync();
        var resultViewModel = JsonConvert.DeserializeObject<SubscriptionViewModel>(rawContent);

        resultViewModel.ShouldNotBeNull();
        resultViewModel!.Id.ShouldBe(subscription.Id);
        //resultViewModel.Active.ShouldBeFalse();

        // Assert 3: Проверка состояния в БД
        using (var scope = _helper.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SubsDbContext>();
            var savedSubscription = db.Subscriptions.SingleOrDefault(s => s.Id == subscription.Id);

            savedSubscription.ShouldNotBeNull();
            savedSubscription!.Active.ShouldBeFalse();
        }
    }

    
    
    
    public async ValueTask DisposeAsync()
    {
        await _helper.ClearTestData();
        _client.Dispose();
        _factory.Dispose();
    }
}
