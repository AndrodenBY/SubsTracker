namespace SubsTracker.UnitTests.GroupMemberService;

public class GroupMemberServiceJoinGroupTests : GroupMemberServiceTestBase
{
    [Fact]
    public async Task JoinGroup_WhenValidModel_ReturnsGroupMember()
    {
        //Arrange
        var userId = Guid.NewGuid();
        var groupId = Guid.NewGuid();

        var createDto = Fixture.Build<CreateGroupMemberDto>()
            .With(dto => dto.UserId, userId)
            .With(dto => dto.GroupId, groupId)
            .Create();

        var createdMemberEntity = Fixture.Build<GroupMember>()
            .With(gm => gm.UserId, userId)
            .With(gm => gm.GroupId, groupId)
            .Create();

        var createdMemberDto = Fixture.Build<GroupMemberDto>()
            .With(dto => dto.UserId, userId)
            .With(dto => dto.GroupId, groupId)
            .Create();

        MemberRepository.GetByPredicate(Arg.Any<Expression<Func<GroupMember, bool>>>(), default)
           .Returns((GroupMember?)null);
        MemberRepository.Create(Arg.Any<GroupMember>(), default)
           .Returns(createdMemberEntity);
        Mapper.Map<GroupMemberDto>(createdMemberEntity)
           .Returns(createdMemberDto);

        //Act
        var result = await Service.JoinGroup(createDto, default);

        //Assert
        result.ShouldNotBeNull();
        result.UserId.ShouldBe(userId);
        result.GroupId.ShouldBe(groupId);
        await MemberRepository.Received(1).Create(Arg.Any<GroupMember>(), default);
    }

    [Fact]
    public async Task JoinGroup_WhenMemberAlreadyExists_ThrowsValidationException()
    {
        //Arrange
        var userId = Guid.NewGuid();
        var groupId = Guid.NewGuid();

        var createDto = Fixture.Build<CreateGroupMemberDto>()
            .With(dto => dto.UserId, userId)
            .With(dto => dto.GroupId, groupId)
            .Create();

        var existingMemberEntity = Fixture.Build<GroupMember>()
            .With(gm => gm.UserId, userId)
            .With(gm => gm.GroupId, groupId)
            .Create();

        MemberRepository.GetByPredicate(Arg.Any<Expression<Func<GroupMember, bool>>>(), default)
           .Returns(existingMemberEntity);

        //Act
        var act = async () => await Service.JoinGroup(createDto, default);

        //Assert
        await act.ShouldThrowAsync<ValidationException>();
    }
}
