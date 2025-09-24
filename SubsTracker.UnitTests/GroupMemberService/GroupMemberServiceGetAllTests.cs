namespace SubsTracker.UnitTests.GroupMemberService;

public class GroupMemberServiceGetAllTests : GroupMemberServiceTestBase
{
    [Fact]
    public async Task GetAll_WhenFilterIsEmpty_ReturnsAllMembers()
    {
        //Arrange
        var members = _fixture.CreateMany<GroupMember>(3).ToList();
        var memberDtos = _fixture.CreateMany<GroupMemberDto>(3).ToList();
        
        var filter = new GroupMemberFilterDto();

        _repository.GetAll(Arg.Any<Expression<Func<GroupMember, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(members);
        _mapper.Map<List<GroupMemberDto>>(Arg.Any<List<GroupMember>>())
            .Returns(memberDtos);
        
        //Act
        var result = await _service.GetAll(filter, default);

        //Assert
        result.ShouldNotBeNull();
        result.ShouldNotBeEmpty();
        result.Count.ShouldBe(3);
    }
    
    [Fact]
    public async Task GetAll_WhenNoMembers_ReturnsEmptyList()
    {
        //Arrange
        var filter = new GroupMemberFilterDto();
        
        _repository.GetAll(Arg.Any<Expression<Func<GroupMember, bool>>>(), Arg.Any<CancellationToken>())
            .Returns([]);
        _mapper.Map<List<GroupMemberDto>>(Arg.Any<List<GroupMember>>()).Returns(new List<GroupMemberDto>());
        
        //Act
        var result = await _service.GetAll(filter, default);
        
        //Assert
        result.ShouldBeEmpty();
    }
    
    [Fact]
    public async Task GetAll_WhenFilteredByName_ReturnsCorrectGroupMember()
    {
        //Arrange
        var memberToFind = _fixture.Create<GroupMember>();
        var memberDto = _fixture.Build<GroupMemberDto>()
            .With(d => d.Id, memberToFind.Id)
            .With(filter => filter.Role, memberToFind.Role)
            .Create();
        
        var filter = new GroupMemberFilterDto { Role = memberToFind.Role };

        _repository.GetAll(Arg.Any<Expression<Func<GroupMember, bool>>>(), default)
            .Returns(new List<GroupMember> { memberToFind });
        _mapper.Map<List<GroupMemberDto>>(Arg.Any<List<GroupMember>>()).Returns(new List<GroupMemberDto> { memberDto });

        //Act
        var result = await _service.GetAll(filter, default);

        //Assert
        await _repository.Received(1).GetAll(
            Arg.Any<Expression<Func<GroupMember, bool>>>(), 
            default
        );

        result.ShouldNotBeNull();
        result.ShouldHaveSingleItem();
        result.Single().Role.ShouldBe(memberToFind.Role);
    }
}
