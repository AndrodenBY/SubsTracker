using AutoFixture;
using AutoMapper;
using DispatchR;
using NSubstitute;
using SubsTracker.BLL.Helpers.Policy;
using SubsTracker.BLL.Interfaces.Cache;
using SubsTracker.BLL.Services;
using SubsTracker.DAL.Interfaces.Repositories;

namespace SubsTracker.UnitTests.TestsBase;

public class MemberServiceTestBase
{
    protected readonly ICacheService CacheService;
    protected readonly IFixture Fixture;
    protected readonly IMapper Mapper;
    protected readonly IMemberRepository MemberRepository;
    protected readonly MemberService Service;
    protected readonly IMediator Mediator;
    
    protected readonly IMemberPolicyChecker MemberPolicyChecker;
    protected readonly IUserRepository UserRepository;
    protected readonly IGroupRepository GroupRepository;

    protected MemberServiceTestBase()
    {
        Fixture = new Fixture();
        Fixture.Customize<DateOnly>(composer => composer.FromFactory(() =>
            DateOnly.FromDateTime(DateTime.Today.AddDays(Fixture.Create<int>()))));
        Fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => Fixture.Behaviors.Remove(b));
        Fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        MemberPolicyChecker = Substitute.For<IMemberPolicyChecker>();
        UserRepository = Substitute.For<IUserRepository>();
        GroupRepository = Substitute.For<IGroupRepository>();
        
        MemberRepository = Substitute.For<IMemberRepository>();
        Mapper = Substitute.For<IMapper>();
        CacheService = Substitute.For<ICacheService>();
        Mediator = Substitute.For<IMediator>();

        Service = new MemberService(
            MemberRepository,
            Mapper,
            CacheService,
            Mediator
        );
    }
}

