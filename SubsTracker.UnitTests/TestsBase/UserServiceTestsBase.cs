using AutoFixture;
using AutoMapper;
using NSubstitute;
using SubsTracker.BLL.Interfaces.Cache;
using SubsTracker.BLL.Services;
using SubsTracker.DAL.Interfaces.Repositories;

namespace SubsTracker.UnitTests.TestsBase;

public class UserServiceTestsBase
{
    protected readonly ICacheService CacheService;
    protected readonly IFixture Fixture;
    protected readonly IMapper Mapper;
    protected readonly UserService Service;
    protected readonly IUserRepository UserRepository;

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

        Service = new UserService(
            UserRepository,
            Mapper,
            CacheService
        );
    }
}
