namespace SubsTracker.UnitTests.UserService;

public class UserServiceTestsBase
{
    protected readonly IFixture Fixture;
    protected readonly IRepository<User> Repository;
    protected readonly IMapper Mapper;
    protected readonly BLL.Services.User.UserService Service;

    protected UserServiceTestsBase()
    {
        Fixture = new Fixture();
        Fixture.Customize<DateOnly>(composer => composer.FromFactory(() =>
           DateOnly.FromDateTime(DateTime.Today.AddDays(Fixture.Create<int>()))));
        Fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
           .ForEach(b => Fixture.Behaviors.Remove(b));
        Fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        Repository = Substitute.For<IRepository<User>>();
        Mapper = Substitute.For<IMapper>();
        Service = new BLL.Services.User.UserService(Repository, Mapper);
    }
}
