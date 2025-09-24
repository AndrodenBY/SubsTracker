namespace SubsTracker.UnitTests.GroupMemberService;

public class GroupMemberServiceJoinGroupTests : GroupMemberServiceTestBase
{
    [Fact]
    public async Task JoinGroup_WhenValidModel_ReturnsGroupMember()
    {
        //Arrange
        var userId = Guid.NewGuid();
        var groupId = Guid.NewGuid();
    
        var createDto = _fixture.Build<CreateGroupMemberDto>()
            .With(dto => dto.UserId, userId)
            .With(dto => dto.GroupId, groupId)
            .Create();
    
        var userEntity = _fixture.Build<User>()
            .With(u => u.Id, userId)
            .Create();

        var groupEntity = _fixture.Build<UserGroup>()
            .With(g => g.Id, groupId)
            .Create();

        var createdMemberEntity = _fixture.Build<GroupMember>()
            .With(gm => gm.UserId, userId)
            .With(gm => gm.GroupId, groupId)
            .Create();
        
        var createdMemberDto = _fixture.Build<GroupMemberDto>()
            .With(dto => dto.UserId, userId)
            .With(dto => dto.GroupId, groupId)
            .Create();
        
        _userRepository.GetById(userId, default)
            .Returns(userEntity);
        _groupRepository.GetById(groupId, default)
            .Returns(groupEntity);
        _repository.GetByPredicate(Arg.Any<Expression<Func<GroupMember, bool>>>(), default)
            .Returns((GroupMember)null);
        _repository.Create(Arg.Any<GroupMember>(), default)
            .Returns(createdMemberEntity);
        _mapper.Map<GroupMemberDto>(createdMemberEntity)
            .Returns(createdMemberDto);

        //Act
        var result = await _service.JoinGroup(createDto, default);

        //Assert
        result.ShouldNotBeNull();
        result.UserId.ShouldBe(userId);
        result.GroupId.ShouldBe(groupId);
        await _repository.Received(1).Create(Arg.Any<GroupMember>(), default);
    }

    [Fact]
    public async Task JoinGroup_WhenMemberAlreadyExists_ThrowsValidationException()
    {
        //Arrange
        var userId = Guid.NewGuid();
        var groupId = Guid.NewGuid();
    
        var createDto = _fixture.Build<CreateGroupMemberDto>()
            .With(dto => dto.UserId, userId)
            .With(dto => dto.GroupId, groupId)
            .Create();
    
        var userEntity = _fixture.Build<User>()
            .With(u => u.Id, userId)
            .Create();

        var groupEntity = _fixture.Build<UserGroup>()
            .With(g => g.Id, groupId)
            .Create();
        
        var existingMemberEntity = _fixture.Build<GroupMember>()
            .With(gm => gm.UserId, userId)
            .With(gm => gm.GroupId, groupId)
            .Create();
    
        // Set up mocks for repository and mapper
        _userRepository.GetById(userId, default)
            .Returns(userEntity);
        _groupRepository.GetById(groupId, default)
            .Returns(groupEntity);
        _repository.GetByPredicate(Arg.Any<Expression<Func<GroupMember, bool>>>(), default)
            .Returns(existingMemberEntity);

        //Act
        var act = async () => await _service.JoinGroup(createDto, default);

        //Assert
        await act.ShouldThrowAsync<ValidationException>();
    }
}
