namespace SubsTracker.UnitTests.GroupMemberService;

public class GroupMemberServiceGetAllTests : GroupMemberServiceTestBase
{
    [Fact]
    public async Task GetAll_WhenFilterIsEmpty_ReturnsAllMembers()
    {
        //Arrange
        var members = Fixture.CreateMany<GroupMember>(3).ToList();
        var memberDtos = Fixture.CreateMany<GroupMemberDto>(3).ToList();

        var filter = new GroupMemberFilterDto();

        MemberRepository.GetAll(Arg.Any<Expression<Func<GroupMember, bool>>>(), default)
           .Returns(members);
        Mapper.Map<List<GroupMemberDto>>(Arg.Any<List<GroupMember>>())
           .Returns(memberDtos);

        //Act
        var result = await Service.GetAll(filter, default);

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

        MemberRepository.GetAll(Arg.Any<Expression<Func<GroupMember, bool>>>(), default)
           .Returns([]);
        Mapper.Map<List<GroupMemberDto>>(Arg.Any<List<GroupMember>>()).Returns(new List<GroupMemberDto>());

        //Act
        var result = await Service.GetAll(filter, default);

        //Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetAll_WhenFilteredByName_ReturnsCorrectGroupMember()
    {
        //Arrange
        var memberToFind = Fixture.Create<GroupMember>();
        var memberDto = Fixture.Build<GroupMemberDto>()
            .With(d => d.Id, memberToFind.Id)
            .With(filter => filter.Role, memberToFind.Role)
            .Create();

        var filter = new GroupMemberFilterDto { Role = memberToFind.Role };

        MemberRepository.GetAll(Arg.Any<Expression<Func<GroupMember, bool>>>(), default)
           .Returns(new List<GroupMember> { memberToFind });
        Mapper.Map<List<GroupMemberDto>>(Arg.Any<List<GroupMember>>()).Returns(new List<GroupMemberDto> { memberDto });

        //Act
        var result = await Service.GetAll(filter, default);

        //Assert
        await MemberRepository.Received(1).GetAll(
            Arg.Any<Expression<Func<GroupMember, bool>>>(),
            default
        );

        result.ShouldNotBeNull();
        result.ShouldHaveSingleItem();
        result.Single().Role.ShouldBe(memberToFind.Role);
    }
}
