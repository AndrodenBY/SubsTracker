using DispatchR;

namespace SubsTracker.UnitTests.UserService;

public class UserServiceTestsBase
{
    protected readonly ICacheService CacheService;
    protected readonly IFixture Fixture;
    protected readonly IMapper Mapper;
    protected readonly UserModelService Service;
    protected readonly IUserRepository UserRepository;
    protected readonly IMediator Mediator;

    protected UserServiceTestsBase()
    {
        Fixture = new Fixture();
        Fixture.Customize<DateOnly>(composer => composer.FromFactory(() =>
            DateOnly.FromDateTime(DateTime.Today.AddDays(Fixture.Create<int>()))));
        Fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => Fixture.Behaviors.Remove(b));
        Fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        UserRepository = Substitute.For<IUserRepository>();
        Mapper = Substitute.For<IMapper>();
        CacheService = Substitute.For<ICacheService>();
        Mediator = Substitute.For<IMediator>();

        Service = new UserModelService(
            UserRepository,
            Mapper,
            CacheService,
            Mediator
        );
    }
}
