using SubsTracker.IntegrationTests.Configuration.WebApplicationFactory;

namespace SubsTracker.IntegrationTests.Helpers;

public abstract class TestHelperBase
{
    private readonly IServiceScopeFactory _scopeFactory;
    protected readonly TestsWebApplicationFactory Factory;
    protected readonly IFixture Fixture;

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

    public IServiceScope CreateScope()
    {
        return _scopeFactory.CreateScope();
    }
}
