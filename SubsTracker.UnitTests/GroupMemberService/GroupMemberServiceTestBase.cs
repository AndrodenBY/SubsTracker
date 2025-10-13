namespace SubsTracker.UnitTests.GroupMemberService;

public class GroupMemberServiceTestBase
{
    protected readonly IFixture _fixture;
    protected readonly IRepository<GroupMember> _repository;
    protected readonly IMapper _mapper;
    protected readonly BLL.Services.User.GroupMemberService _service;

    protected GroupMemberServiceTestBase()
    {
        _fixture = new Fixture();
        _fixture.Customize<DateOnly>(composer => composer.FromFactory(() =>
            DateOnly.FromDateTime(DateTime.Today.AddDays(_fixture.Create<int>()))));
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        
        _repository = Substitute.For<IRepository<GroupMember>>();
        _mapper = Substitute.For<IMapper>();
        
        _service = new BLL.Services.User.GroupMemberService(
            _repository, 
            _mapper
        );
    }
}
