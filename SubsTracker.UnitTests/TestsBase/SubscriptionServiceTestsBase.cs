using AutoFixture;
using AutoMapper;
using DispatchR;
using NSubstitute;
using SubsTracker.BLL.Interfaces.Cache;
using SubsTracker.BLL.Services;
using SubsTracker.DAL.Interfaces.Repositories;
using SubsTracker.Messaging.Interfaces;

namespace SubsTracker.UnitTests.TestsBase;

public class SubscriptionServiceTestsBase
{
    protected readonly ICacheService CacheService;
    protected readonly IFixture Fixture;
    protected readonly IMapper Mapper;
    protected readonly SubscriptionService Service;
    protected readonly ISubscriptionRepository SubscriptionRepository;
    protected readonly IUserRepository UserRepository;
    protected readonly IMediator Mediator;

    protected SubscriptionServiceTestsBase()
    {
        Fixture = new Fixture();
        Fixture.Customize<DateOnly>(composer => composer.FromFactory(() =>
            DateOnly.FromDateTime(DateTime.Today.AddDays(Fixture.Create<int>()))));
        Fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => Fixture.Behaviors.Remove(b));
        Fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        SubscriptionRepository = Substitute.For<ISubscriptionRepository>();
        UserRepository = Substitute.For<IUserRepository>();
        Mapper = Substitute.For<IMapper>();
        CacheService = Substitute.For<ICacheService>();
        Mediator = Substitute.For<IMediator>();

        Service = new SubscriptionService(
            SubscriptionRepository,
            UserRepository,
            Mapper,
            CacheService,
            Mediator
        );
    }
}
