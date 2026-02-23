using AutoFixture;
using AutoMapper;
using NSubstitute;
using SubsTracker.BLL.Interfaces;
using SubsTracker.BLL.Interfaces.Cache;
using SubsTracker.BLL.Services;
using SubsTracker.DAL.Interfaces.Repositories;

namespace SubsTracker.UnitTests.TestsBase;

public class GroupServiceTestsBase
{
    private readonly IMemberService _memberService;
    protected readonly ICacheService CacheService;
    protected readonly IFixture Fixture;
    protected readonly IGroupRepository GroupRepository;
    protected readonly IMapper Mapper;
    protected readonly GroupService Service;
    protected readonly ISubscriptionRepository SubscriptionRepository;
    protected readonly IUserRepository UserRepository;

    protected GroupServiceTestsBase()
    {
        Fixture = new Fixture();
        Fixture.Customize<DateOnly>(composer => composer.FromFactory(() =>
            DateOnly.FromDateTime(DateTime.Today.AddDays(Fixture.Create<int>()))));
        Fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => Fixture.Behaviors.Remove(b));
        Fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        GroupRepository = Substitute.For<IGroupRepository>();
        UserRepository = Substitute.For<IUserRepository>();
        SubscriptionRepository = Substitute.For<ISubscriptionRepository>();
        Mapper = Substitute.For<IMapper>();
        _memberService = Substitute.For<IMemberService>();
        CacheService = Substitute.For<ICacheService>();

        Service = new GroupService(
            GroupRepository,
            UserRepository,
            SubscriptionRepository,
            _memberService,
            Mapper,
            CacheService
        );
    }
}
