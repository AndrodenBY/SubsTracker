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

        Repository.GetByPredicate(Arg.Any<Expression<Func<GroupMember, bool>>>(), default)
           .Returns(memberToDelete);
        Repository.GetById(Arg.Any<Guid>(), default).Returns(memberToDelete);
        Repository.Delete(memberToDelete, default)
           .Returns(true);

        //Act
        var result = await Service.LeaveGroup(groupId, userId, default);

        //Assert
        result.ShouldBeTrue();
        await Repository.Received(1).GetByPredicate(Arg.Any<Expression<Func<GroupMember, bool>>>(), default);
        await Repository.Received(1).Delete(memberToDelete, default);
    }

    [Fact]
    public async Task LeaveGroup_WhenNotTheMemberOfTheGroup_ThrowsNotFoundException()
    {
        //Arrange
        var userId = Guid.NewGuid();
        var groupId = Guid.NewGuid();

        Repository.GetByPredicate(Arg.Any<Expression<Func<GroupMember, bool>>>(), default)
           .Returns((GroupMember?)null);

        //Act
        var act = async () => await Service.LeaveGroup(groupId, userId, default);

        //Assert
        await act.ShouldThrowAsync<NotFoundException>();
        await Repository.Received(1).GetByPredicate(Arg.Any<Expression<Func<GroupMember, bool>>>(), default);
        await Repository.DidNotReceive().Delete(Arg.Any<GroupMember>(), default);
    }
}
