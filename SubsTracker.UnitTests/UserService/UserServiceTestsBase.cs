namespace SubsTracker.UnitTests.UserService;

public class UserServiceTestsBase
{
    protected readonly IFixture _fixture;
    protected readonly IRepository<User> _repository;
    protected readonly IMapper _mapper;
    protected readonly BLL.Services.User.UserService _service;

    protected UserServiceTestsBase()
    {
        _fixture = new Fixture();
        _fixture.Customize<DateOnly>(composer => composer.FromFactory(() =>
            DateOnly.FromDateTime(DateTime.Today.AddDays(_fixture.Create<int>()))));
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        
        _repository = Substitute.For<IRepository<User>>();
        _mapper = Substitute.For<IMapper>();
        _service = new BLL.Services.User.UserService(_repository, _mapper);
    }
}
