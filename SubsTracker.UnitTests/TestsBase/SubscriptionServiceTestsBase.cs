using AutoFixture;
using AutoMapper;
using NSubstitute;
using SubsTracker.BLL.Interfaces.Cache;
using SubsTracker.BLL.Services;
using SubsTracker.DAL.Interfaces.Repositories;
using SubsTracker.Messaging.Interfaces;

namespace SubsTracker.UnitTests.TestsBase;

public class SubscriptionServiceTestsBase
{
    protected readonly ICacheAccessService CacheAccessService;
    protected readonly ICacheService CacheService;
    protected readonly IFixture Fixture;
    protected readonly ISubscriptionHistoryRepository HistoryRepository;
    protected readonly IMapper Mapper;
    protected readonly IMessageService MessageService;
    protected readonly SubscriptionService Service;
    protected readonly ISubscriptionRepository SubscriptionRepository;
    protected readonly IUserRepository UserRepository;

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
        MessageService = Substitute.For<IMessageService>();
        Mapper = Substitute.For<IMapper>();
        HistoryRepository = Substitute.For<ISubscriptionHistoryRepository>();
        CacheService = Substitute.For<ICacheService>();
        CacheAccessService = Substitute.For<ICacheAccessService>();

        Service = new SubscriptionService(
            SubscriptionRepository,
            MessageService,
            Mapper,
            UserRepository,
            HistoryRepository,
            CacheService,
            CacheAccessService
        );
    }
}
