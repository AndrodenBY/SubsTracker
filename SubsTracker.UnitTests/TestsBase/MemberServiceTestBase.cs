using AutoFixture;
using AutoMapper;
using NSubstitute;
using SubsTracker.BLL.Interfaces.Cache;
using SubsTracker.DAL.Interfaces.Repositories;
using SubsTracker.Messaging.Interfaces;

namespace SubsTracker.UnitTests.TestsBase;

public class MemberServiceTestBase
{
    protected readonly ICacheAccessService CacheAccessService;
    protected readonly ICacheService CacheService;
    protected readonly IFixture Fixture;
    protected readonly IMapper Mapper;
    protected readonly IMemberRepository MemberRepository;
    protected readonly IMessageService MessageService;
    protected readonly BLL.Services.MemberService Service;

    protected MemberServiceTestBase()
    {
        Fixture = new Fixture();
        Fixture.Customize<DateOnly>(composer => composer.FromFactory(() =>
            DateOnly.FromDateTime(DateTime.Today.AddDays(Fixture.Create<int>()))));
        Fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => Fixture.Behaviors.Remove(b));
        Fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        MemberRepository = Substitute.For<IMemberRepository>();
        MessageService = Substitute.For<IMessageService>();
        Mapper = Substitute.For<IMapper>();
        CacheService = Substitute.For<ICacheService>();
        CacheAccessService = Substitute.For<ICacheAccessService>();

        Service = new BLL.Services.MemberService(
            MemberRepository,
            MessageService,
            Mapper,
            CacheService,
            CacheAccessService
        );
    }
}
