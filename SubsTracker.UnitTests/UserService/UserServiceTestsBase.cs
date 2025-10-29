namespace SubsTracker.UnitTests.UserService;

public class UserServiceTestsBase
{
    protected readonly IFixture Fixture;
    protected readonly IRepository<User> UserRepository;
    protected readonly IMapper Mapper;
    protected readonly UserModelService Service;
    protected readonly ICacheService CacheService;

    protected UserServiceTestsBase()
    {
        Fixture = new Fixture();
        Fixture.Customize<DateOnly>(composer => composer.FromFactory(() =>
           DateOnly.FromDateTime(DateTime.Today.AddDays(Fixture.Create<int>()))));
        Fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
           .ForEach(b => Fixture.Behaviors.Remove(b));
        Fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        UserRepository = Substitute.For<IRepository<User>>();
        Mapper = Substitute.For<IMapper>();
        CacheService = Substitute.For<ICacheService>();
        
        Service = new UserModelService(
            UserRepository,
            Mapper,
            CacheService
        );
    }
}
