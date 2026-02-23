using System.Text.Json;
using System.Text.Json.Serialization;
using AutoFixture;
using Microsoft.Extensions.DependencyInjection;
using SubsTracker.DAL;
using SubsTracker.IntegrationTests.Configuration;

namespace SubsTracker.IntegrationTests.Helpers;

public abstract class TestHelperBase
{
    private readonly IServiceScopeFactory _scopeFactory;
    protected readonly TestsWebApplicationFactory Factory;
    protected readonly IFixture Fixture;

    public static readonly JsonSerializerOptions DefaultJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };
    
    protected TestHelperBase(TestsWebApplicationFactory factory)
    {
        Factory = factory;
        _scopeFactory = factory.Services.GetRequiredService<IServiceScopeFactory>();

        Fixture = new Fixture();
        Fixture.Customize<DateOnly>(composer => composer.FromFactory(() =>
            DateOnly.FromDateTime(DateTime.Today.AddDays(Fixture.Create<int>()))));
        Fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => Fixture.Behaviors.Remove(b));
        Fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }

    protected IServiceScope CreateScope()
    {
        return _scopeFactory.CreateScope();
    }
    
    public async Task<TEntity?> FindEntityAsync<TEntity>(params object[] keyValues) where TEntity : class
    {
        using var scope = CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SubsDbContext>();
        return await db.Set<TEntity>().FindAsync(keyValues);
    }
}
