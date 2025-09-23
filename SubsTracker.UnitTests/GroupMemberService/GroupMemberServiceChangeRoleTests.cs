namespace SubsTracker.UnitTests.GroupMemberService;

public class GroupMemberServiceChangeRoleTests : GroupMemberServiceTestBase
{
    [Fact]
    public async Task ChangeRole_WhenNotAdmin_ChangeRole()
    {
        //Arrange
        var memberId = Guid.NewGuid();
        var memberEntity = _fixture.Build<GroupMember>()
            .With(member => member.Id, memberId)
            .With(member => member.Role, MemberRole.Participant)
            .Create();
        
        var updatedMemberEntity = _fixture.Build<GroupMember>()
            .With(member => member.Id, memberId)
            .With(member => member.Role, MemberRole.Moderator)
            .Create();
        
        var memberDto = _fixture.Build<GroupMemberDto>()
            .With(dto => dto.Id, memberId)
            .With(dto => dto.Role, MemberRole.Moderator)
            .Create();
        
        _repository.GetById(memberId, default).Returns(memberEntity);
        _mapper.Map(Arg.Any<UpdateGroupMemberDto>(), memberEntity).Returns(memberEntity);
        _repository.Update(memberEntity, default).Returns(updatedMemberEntity);
        _mapper.Map<GroupMemberDto>(updatedMemberEntity).Returns(memberDto);

        //Act
        var result = await _service.ChangeRole(memberId, default);

        //Assert
        result.ShouldNotBeNull();
        result.Role.ShouldBe(MemberRole.Moderator);
        result.Role.ShouldNotBe(memberEntity.Role);
        result.Id.ShouldBe(memberEntity.Id);
        await _repository.Received(1).Update(Arg.Any<GroupMember>(), default);
    }
    
    [Fact]
    public async Task ChangeRole_WhenAdmin_ThrowsInvalidOperationException()
    {
        //Arrange
        var memberEntity = _fixture.Build<GroupMember>()
            .With(member => member.Role, MemberRole.Admin)
            .Create();
        
        var updateDto = new UpdateGroupMemberDto { Id = memberEntity.Id, Role = MemberRole.Moderator };
        
        _repository.GetById(memberEntity.Id, default).Returns(memberEntity);
    
        //Act
        var result = async () => await _service.ChangeRole(updateDto.Id, default);
        
        //Assert
        await result.ShouldThrowAsync<InvalidOperationException>();
    }
    
}
