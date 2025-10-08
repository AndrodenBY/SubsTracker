namespace SubsTracker.IntegrationTests.Helpers;

public abstract class TestHelperBase
{
    public readonly TestsWebApplicationFactory _factory;
    public readonly IServiceScopeFactory _scopeFactory;
    public readonly IFixture _fixture;

    protected TestHelperBase(TestsWebApplicationFactory factory)
    {
        _factory = factory;
        _scopeFactory = factory.Services.GetRequiredService<IServiceScopeFactory>();

        _fixture = new Fixture();
        _fixture.Customize<DateOnly>(composer => composer.FromFactory(() =>
            DateOnly.FromDateTime(DateTime.Today.AddDays(_fixture.Create<int>()))));
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }

    public IServiceScope CreateScope() => _scopeFactory.CreateScope();
}
