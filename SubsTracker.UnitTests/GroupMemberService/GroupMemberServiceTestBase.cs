namespace SubsTracker.UnitTests.GroupMemberService;

public class GroupMemberServiceTestBase
{
    protected readonly IFixture Fixture;
    protected readonly IRepository<GroupMember> Repository;
    protected readonly IMapper Mapper;
    protected readonly BLL.Services.User.GroupMemberService Service;

    protected GroupMemberServiceTestBase()
    {
        Fixture = new Fixture();
        Fixture.Customize<DateOnly>(composer => composer.FromFactory(() =>
             DateOnly.FromDateTime(DateTime.Today.AddDays(Fixture.Create<int>()))));
        Fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
             .ForEach(b => Fixture.Behaviors.Remove(b));
        Fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        Repository = Substitute.For<IRepository<GroupMember>>();
        Mapper = Substitute.For<IMapper>();

        Service = new BLL.Services.User.GroupMemberService(
            Repository,
            Mapper
       );
    }
}
