namespace SubsTracker.UnitTests.GroupMemberService;

public class GroupMemberServiceChangeRoleTests : GroupMemberServiceTestBase
{
    [Fact]
    public async Task ChangeRole_WhenNotAdmin_ChangeRole()
    {
        //Arrange
        var memberId = Guid.NewGuid();
        var memberEntity = Fixture.Build<GroupMember>()
            .With(member => member.Id, memberId)
            .With(member => member.Role, MemberRole.Participant)
            .Create();

        var updatedMemberEntity = Fixture.Build<GroupMember>()
            .With(member => member.Id, memberId)
            .With(member => member.Role, MemberRole.Moderator)
            .Create();

        var memberDto = Fixture.Build<GroupMemberDto>()
            .With(dto => dto.Id, memberId)
            .With(dto => dto.Role, MemberRole.Moderator)
            .Create();

        MemberRepository.GetFullInfoById(memberId, default).Returns(memberEntity);
        Mapper.Map(Arg.Any<UpdateGroupMemberDto>(), memberEntity).Returns(memberEntity);
        MemberRepository.Update(memberEntity, default).Returns(updatedMemberEntity);
        Mapper.Map<GroupMemberDto>(updatedMemberEntity).Returns(memberDto);

        //Act
        var result = await Service.ChangeRole(memberId, default);

        //Assert
        result.ShouldNotBeNull();
        result.Role.ShouldBe(MemberRole.Moderator);
        result.Role.ShouldNotBe(memberEntity.Role);
        result.Id.ShouldBe(memberEntity.Id);
        await MemberRepository.Received(1).Update(Arg.Any<GroupMember>(), default);
        await MessageService.Received(1)
            .NotifyMemberChangedRole(Arg.Is<MemberChangedRoleEvent>(memberEvent => memberEvent.Id == memberId),
                default);
    }

    [Fact]
    public async Task ChangeRole_WhenAdmin_ThrowsInvalidOperationException()
    {
        //Arrange
        var memberEntity = Fixture.Build<GroupMember>()
            .With(member => member.Role, MemberRole.Admin)
            .Create();

        var updateDto = Fixture.Build<UpdateGroupMemberDto>()
            .With(dto => dto.Id, memberEntity.Id)
            .With(dto => dto.Role, MemberRole.Moderator)
            .Create();

        MemberRepository.GetFullInfoById(memberEntity.Id, default).Returns(memberEntity);

        //Act
        var result = async () => await Service.ChangeRole(updateDto.Id, default);

        //Assert
        await result.ShouldThrowAsync<InvalidOperationException>();
    }
}
