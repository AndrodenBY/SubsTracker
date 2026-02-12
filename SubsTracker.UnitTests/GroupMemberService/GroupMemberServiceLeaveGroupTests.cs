namespace SubsTracker.UnitTests.GroupMemberService;

public class GroupMemberServiceLeaveGroupTests : GroupMemberServiceTestBase
{
    [Fact]
    public async Task LeaveGroup_WhenModelIsValid_ReturnsTrue()
    {
        //Arrange
        var userId = Guid.NewGuid();
        var groupId = Guid.NewGuid();

        var memberToDelete = Fixture.Build<GroupMember>()
            .With(m => m.UserId, userId)
            .With(m => m.GroupId, groupId)
            .Create();

        MemberRepository.GetByPredicateFullInfo(Arg.Any<Expression<Func<GroupMember, bool>>>(), default)
            .Returns(memberToDelete);
        MemberRepository.GetFullInfoById(Arg.Any<Guid>(), default).Returns(memberToDelete);
        MemberRepository.Delete(memberToDelete, default)
            .Returns(true);

        //Act
        var result = await Service.LeaveGroup(groupId, userId, default);

        //Assert
        result.ShouldBeTrue();
        await MemberRepository.Received(1)
            .GetByPredicateFullInfo(Arg.Any<Expression<Func<GroupMember, bool>>>(), default);
        await MemberRepository.Received(1).Delete(memberToDelete, default);
        await MessageService.Received(1)
            .NotifyMemberLeftGroup(Arg.Is<MemberLeftGroupEvent>(memberEvent => memberEvent.GroupId == groupId),
                default);
    }

    [Fact]
    public async Task LeaveGroup_WhenNotTheMemberOfTheGroup_ThrowsNotFoundException()
    {
        //Arrange
        var userId = Guid.NewGuid();
        var groupId = Guid.NewGuid();

        MemberRepository.GetByPredicateFullInfo(Arg.Any<Expression<Func<GroupMember, bool>>>(), default)
            .Returns((GroupMember?)null);

        //Act
        var act = async () => await Service.LeaveGroup(groupId, userId, default);

        //Assert
        await act.ShouldThrowAsync<UnknownIdentifierException>();
        await MemberRepository.Received(1)
            .GetByPredicateFullInfo(Arg.Any<Expression<Func<GroupMember, bool>>>(), default);
        await MemberRepository.DidNotReceive().Delete(Arg.Any<GroupMember>(), default);
    }

    [Fact]
    public async Task LeaveGroup_WhenSuccessful_DeletesAndNotifies()
    {
        //Arrange
        var userId = Guid.NewGuid();
        var groupId = Guid.NewGuid();
        var memberId = Guid.NewGuid();

        var memberToDelete = Fixture.Build<GroupMember>()
            .With(m => m.Id, memberId)
            .With(m => m.UserId, userId)
            .With(m => m.GroupId, groupId)
            .Create();

        MemberRepository.GetByPredicateFullInfo(
                Arg.Is<Expression<Func<GroupMember, bool>>>(expr => expr.Compile()(memberToDelete)), default)
            .Returns(memberToDelete);

        MemberRepository.Delete(memberToDelete, default)
            .Returns(true);

        var memberCacheKey = RedisKeySetter.SetCacheKey<GroupMemberDto>(memberId);

        //Act
        var result = await Service.LeaveGroup(groupId, userId, default);

        //Assert
        result.ShouldBeTrue();

        await MemberRepository.Received(1)
            .GetByPredicateFullInfo(Arg.Any<Expression<Func<GroupMember, bool>>>(), default);
        await MemberRepository.Received(1).Delete(memberToDelete, default);

        await CacheAccessService.Received(1)
            .RemoveData(Arg.Is<List<string>>(keys => keys.Contains(memberCacheKey) && keys.Count == 1), default);

        await MessageService.Received(1)
            .NotifyMemberLeftGroup(
                Arg.Is<MemberLeftGroupEvent>(e =>
                    e.Id == memberId &&
                    e.GroupId == groupId
                ),
                default
            );
    }
}
