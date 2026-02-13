namespace SubsTracker.UnitTests.GroupMemberService;

public class GroupMemberServiceTests : GroupMemberServiceTestBase
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
        await act.ShouldThrowAsync<InvalidRequestDataException>();
    }
    
    [Fact]
    public async Task GetFullInfoById_WhenCacheHit_ReturnsCachedDataAndSkipsRepo()
    {
        //Arrange
        var memberId = Fixture.Create<Guid>();
        var cacheKey = $"{memberId}:{nameof(GroupMemberDto)}";

        var cachedDto = Fixture.Build<GroupMemberDto>()
            .With(dto => dto.Id, memberId)
            .With(dto => dto.Role, MemberRole.Admin)
            .Create();

        CacheService.CacheDataWithLock(
            cacheKey,
            Arg.Any<TimeSpan>(),
            Arg.Any<Func<Task<GroupMemberDto?>>>(),
            default
        ).Returns(cachedDto);

        //Act
        var result = await Service.GetFullInfoById(memberId, default);

        //Assert
        result.ShouldBe(cachedDto);
        result?.Role.ShouldBe(MemberRole.Admin);

        await MemberRepository.DidNotReceive().GetFullInfoById(Arg.Any<Guid>(), default);
        await CacheService.Received(1).CacheDataWithLock(
            cacheKey,
            Arg.Any<TimeSpan>(),
            Arg.Any<Func<Task<GroupMemberDto?>>>(),
            default
        );
    }

    [Fact]
    public async Task GetFullInfoById_WhenCacheMiss_FetchesFromRepoAndCaches()
    {
        //Arrange
        var memberId = Fixture.Create<Guid>();
        var cacheKey = $"{memberId}:{nameof(GroupMemberDto)}";

        var memberEntity = Fixture.Create<GroupMember>();
        memberEntity.Id = memberId;

        var memberDto = Fixture.Build<GroupMemberDto>()
            .With(dto => dto.Id, memberId)
            .Create();

        MemberRepository.GetFullInfoById(memberId, default).Returns(memberEntity);
        Mapper.Map<GroupMemberDto>(memberEntity).Returns(memberDto);

        CacheService.CacheDataWithLock(
            cacheKey,
            Arg.Any<TimeSpan>(),
            Arg.Any<Func<Task<GroupMemberDto?>>>(),
            default
        )!.Returns(callInfo =>
        {
            var factory = callInfo.Arg<Func<Task<GroupMemberDto>>>();
            return factory();
        });

        //Act
        var result = await Service.GetFullInfoById(memberId, default);

        //Assert
        result.ShouldBe(memberDto);
        result?.Id.ShouldBe(memberId);

        await MemberRepository.Received(1).GetFullInfoById(memberId, default);
        Mapper.Received(1).Map<GroupMemberDto>(memberEntity);

        await CacheService.Received(1).CacheDataWithLock(
            cacheKey,
            Arg.Any<TimeSpan>(),
            Arg.Any<Func<Task<GroupMemberDto?>>>(),
            default
        );
    }
    
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
