using SubsTracker.Domain.Exceptions;

namespace SubsTracker.UnitTests.GroupMemberService;

public class GroupMemberServiceLeaveGroupTests : GroupMemberServiceTestBase
{
    [Fact]
    public async Task LeaveGroup_WhenModelIsValid_ReturnsTrue()
    {
        //Arrange
        var userId = Guid.NewGuid();
        var groupId = Guid.NewGuid();
    
        var memberToDelete = _fixture.Build<GroupMember>()
            .With(m => m.UserId, userId)
            .With(m => m.GroupId, groupId)
            .Create();
        
        _repository.GetByPredicate(Arg.Any<Expression<Func<GroupMember, bool>>>(), default)
            .Returns(memberToDelete);
        _repository.GetById(Arg.Any<Guid>(), default).Returns(memberToDelete);
        _repository.Delete(memberToDelete, default)
            .Returns(true);
    
        //Act
        var result = await _service.LeaveGroup(groupId, userId, default);
    
        //Assert
        result.ShouldBeTrue();
        await _repository.Received(1).GetByPredicate(Arg.Any<Expression<Func<GroupMember, bool>>>(), default);
        await _repository.Received(1).Delete(memberToDelete, default);
    }

    [Fact]
    public async Task LeaveGroup_WhenNotTheMemberOfTheGroup_ThrowsNotFoundException()
    {
        //Arrange
        var userId = Guid.NewGuid();
        var groupId = Guid.NewGuid();
        
        _repository.GetByPredicate(Arg.Any<Expression<Func<GroupMember, bool>>>(), default)
            .Returns((GroupMember)null);
    
        //Act
        var act = async () => await _service.LeaveGroup(groupId, userId, default);
    
        //Assert
        await act.ShouldThrowAsync<NotFoundException>();
        await _repository.Received(1).GetByPredicate(Arg.Any<Expression<Func<GroupMember, bool>>>(), default);
        await _repository.DidNotReceive().Delete(Arg.Any<GroupMember>(), default);
    }
}
