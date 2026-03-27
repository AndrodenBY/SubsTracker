using AutoFixture;
using AutoMapper;
using NSubstitute;
using SubsTracker.BLL.Services;
using SubsTracker.DAL.Interfaces.Repositories;

namespace SubsTracker.UnitTests.TestsBase;

public class SubscriptionHistoryServiceTestBase
{
    protected ISubscriptionHistoryRepository SubscriptionHistoryRepository;
    protected readonly IFixture Fixture;
    protected IMapper Mapper;
    protected readonly SubscriptionHistoryService SubscriptionHistoryService;

    protected SubscriptionHistoryServiceTestBase()
    {
        Fixture = new Fixture();
        Fixture.Customize<DateOnly>(composer => composer.FromFactory(() =>
            DateOnly.FromDateTime(DateTime.Today.AddDays(Fixture.Create<int>()))));
        Fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => Fixture.Behaviors.Remove(b));
        Fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        SubscriptionHistoryRepository = Substitute.For<ISubscriptionHistoryRepository>();
        Mapper = Substitute.For<IMapper>();
        
        SubscriptionHistoryService = new SubscriptionHistoryService(
            SubscriptionHistoryRepository, 
            Mapper
        );
    }
}
