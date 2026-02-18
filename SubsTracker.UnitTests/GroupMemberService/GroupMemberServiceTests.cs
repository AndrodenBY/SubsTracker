using SubsTracker.BLL.Handlers.Signals.Member;
using SubsTracker.Domain.Pagination;

namespace SubsTracker.UnitTests.GroupMemberService;

public class GroupMemberServiceTests : GroupMemberServiceTestBase
{
    [Fact]
    public async Task LeaveGroup_WhenModelIsValid_ReturnsTrue()
    {
        //Arrange
        var userId = Guid.NewGuid();
        var groupId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;

        var memberToDelete = Fixture.Build<GroupMember>()
            .With(m => m.UserId, userId)
            .With(m => m.GroupId, groupId)
            .Create();

        MemberRepository.GetByPredicateFullInfo(Arg.Any<Expression<Func<GroupMember, bool>>>(), cancellationToken)
            .Returns(memberToDelete);
    
        MemberRepository.Delete(memberToDelete, cancellationToken)
            .Returns(true);

        //Act
        var result = await Service.LeaveGroup(groupId, userId, cancellationToken);

        //Assert
        result.ShouldBeTrue();
        await MemberRepository.Received(1)
            .Delete(memberToDelete, cancellationToken);
        await Mediator.Received(1).Publish(
            Arg.Is<MemberLeftSignal>(signal => 
                signal.GroupId == groupId &&
                signal.GroupName == memberToDelete.Group.Name &&
                signal.UserEmail == memberToDelete.User.Email),
            cancellationToken);
    }

    [Fact]
    public async Task LeaveGroup_WhenNotTheMemberOfTheGroup_ThrowsNotFoundException()
    {
        //Arrange
        var userId = Guid.NewGuid();
        var groupId = Guid.NewGuid();

        MemberRepository.GetByPredicateFullInfo(Arg.Any<Expression<Func<GroupMember, bool>>>(), CancellationToken.None)
            .Returns((GroupMember?)null);

        //Act
        var act = async () => await Service.LeaveGroup(groupId, userId, CancellationToken.None);

        //Assert
        await act.ShouldThrowAsync<UnknownIdentifierException>();
        await MemberRepository.Received(1)
            .GetByPredicateFullInfo(Arg.Any<Expression<Func<GroupMember, bool>>>(), CancellationToken.None);
        await MemberRepository.DidNotReceive().Delete(Arg.Any<GroupMember>(), CancellationToken.None);
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
                Arg.Is<Expression<Func<GroupMember, bool>>>(expr => expr.Compile()(memberToDelete)), CancellationToken.None)
            .Returns(memberToDelete);

        MemberRepository.Delete(memberToDelete, CancellationToken.None)
            .Returns(true);

        //Act
        var result = await Service.LeaveGroup(groupId, userId, CancellationToken.None);

        //Assert
        result.ShouldBeTrue();

        await MemberRepository.Received(1)
            .GetByPredicateFullInfo(Arg.Any<Expression<Func<GroupMember, bool>>>(), CancellationToken.None);
        await MemberRepository.Received(1).Delete(memberToDelete, CancellationToken.None);
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

        MemberRepository.GetByPredicate(Arg.Any<Expression<Func<GroupMember, bool>>>(), CancellationToken.None)
            .Returns((GroupMember?)null);
        MemberRepository.Create(Arg.Any<GroupMember>(), CancellationToken.None)
            .Returns(createdMemberEntity);
        Mapper.Map<GroupMemberDto>(createdMemberEntity)
            .Returns(createdMemberDto);

        //Act
        var result = await Service.JoinGroup(createDto, CancellationToken.None);

        //Assert
        result.ShouldNotBeNull();
        result.UserId.ShouldBe(userId);
        result.GroupId.ShouldBe(groupId);
        await MemberRepository.Received(1).Create(Arg.Any<GroupMember>(), CancellationToken.None);
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

        MemberRepository.GetByPredicate(Arg.Any<Expression<Func<GroupMember, bool>>>(), CancellationToken.None)
            .Returns(existingMemberEntity);

        //Act
        var act = async () => await Service.JoinGroup(createDto, CancellationToken.None);

        //Assert
        await act.ShouldThrowAsync<InvalidRequestDataException>();
    }
    
    [Fact]
    public async Task GetFullInfoById_WhenMemberExists_ReturnsMappedDto()
    {
        //Arrange
        var memberId = Guid.NewGuid();
        var ct = CancellationToken.None;
    
        var memberEntity = Fixture.Build<GroupMember>()
            .With(m => m.Id, memberId)
            .Create();
        
        var expectedDto = Fixture.Build<GroupMemberDto>()
            .With(dto => dto.Id, memberId)
            .Create();

        MemberRepository.GetFullInfoById(memberId, ct)
            .Returns(memberEntity);
        
        Mapper.Map<GroupMemberDto>(memberEntity)
            .Returns(expectedDto);

        //Act
        var result = await Service.GetFullInfoById(memberId, ct);

        //Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(memberId);
    
        await MemberRepository.Received(1).GetFullInfoById(memberId, ct);
        await CacheService.DidNotReceiveWithAnyArgs().CacheDataWithLock(
            Arg.Any<string>(), Arg.Any<Func<Task<GroupMemberDto?>>>(), Arg.Any<CancellationToken>());
    }
    
    [Fact]
    public async Task GetAll_WhenFilterIsEmpty_ReturnsPaginatedMembers()
    {
        //Arrange
        var members = Fixture.CreateMany<GroupMember>(3).ToList();
        var memberDtos = Fixture.CreateMany<GroupMemberDto>(3).ToList();
        
        var paginatedMembers = new PaginatedList<GroupMember>(members, 1, 10, 1, 3);
    
        var filter = new GroupMemberFilterDto();
        var paginationParams = new PaginationParameters { PageNumber = 1, PageSize = 10 };
        
        MemberRepository.GetAll(
                Arg.Any<Expression<Func<GroupMember, bool>>>(), 
                Arg.Any<PaginationParameters>(), 
                Arg.Any<CancellationToken>())
            .Returns(paginatedMembers);
        
        Mapper.Map<GroupMemberDto>(Arg.Any<GroupMember>())
            .Returns(memberDtos[0], memberDtos[1], memberDtos[2]);

        //Act
        var result = await Service.GetAll(filter, paginationParams, CancellationToken.None);

        //Assert
        result.ShouldNotBeNull();
        result.Items.ShouldNotBeEmpty();
        result.Items.Count.ShouldBe(3);
        result.TotalCount.ShouldBe(3);
        result.PageNumber.ShouldBe(1);
    }

    [Fact]
    public async Task GetAll_WhenNoMembers_ReturnsEmptyPaginatedList()
    {
        //Arrange
        var filter = new GroupMemberFilterDto();
        var paginationParams = new PaginationParameters { PageNumber = 1, PageSize = 10 };
        
        var emptyPaginatedMembers = new PaginatedList<GroupMember>([], 1, 10, 0, 0);

        MemberRepository.GetAll(
                Arg.Any<Expression<Func<GroupMember, bool>>>(), 
                Arg.Any<PaginationParameters>(), 
                Arg.Any<CancellationToken>())
            .Returns(emptyPaginatedMembers);

        //Act
        var result = await Service.GetAll(filter, paginationParams, CancellationToken.None);

        //Assert
        result.ShouldNotBeNull();
        result.Items.ShouldBeEmpty();
        result.TotalCount.ShouldBe(0);
        result.PageCount.ShouldBe(0);
    }

    [Fact]
    public async Task GetAll_WhenFilteredByRole_ReturnsCorrectGroupMember()
    {
        //Arrange
        var memberToFind = Fixture.Create<GroupMember>();
        var memberDto = Fixture.Build<GroupMemberDto>()
            .With(d => d.Id, memberToFind.Id)
            .With(d => d.Role, memberToFind.Role)
            .Create();

        var filter = new GroupMemberFilterDto { Role = memberToFind.Role };
        var paginationParams = new PaginationParameters { PageNumber = 1, PageSize = 10 };
        
        var paginatedResult = new PaginatedList<GroupMember>(
            [memberToFind],
            PageNumber: 1, 
            PageSize: 10, 
            PageCount: 1, 
            TotalCount: 1
        );
        
        MemberRepository.GetAll(
                Arg.Any<Expression<Func<GroupMember, bool>>>(),
                Arg.Any<PaginationParameters>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(paginatedResult);
        
        Mapper.Map<GroupMemberDto>(memberToFind).Returns(memberDto);

        //Act
        var result = await Service.GetAll(filter, paginationParams, CancellationToken.None);

        //Assert
        await MemberRepository.Received(1).GetAll(
            Arg.Any<Expression<Func<GroupMember, bool>>>(),
            Arg.Is<PaginationParameters>(p => p.PageNumber == 1 && p.PageSize == 10),
            Arg.Any<CancellationToken>()
        );

        result.ShouldNotBeNull();
        result.Items.ShouldHaveSingleItem();
        result.Items.Single().Role.ShouldBe(memberToFind.Role);
        result.TotalCount.ShouldBe(1);
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

        MemberRepository.GetFullInfoById(memberId, CancellationToken.None).Returns(memberEntity);
        Mapper.Map(Arg.Any<UpdateGroupMemberDto>(), memberEntity).Returns(memberEntity);
        MemberRepository.Update(memberEntity, CancellationToken.None).Returns(updatedMemberEntity);
        Mapper.Map<GroupMemberDto>(updatedMemberEntity).Returns(memberDto);

        //Act
        var result = await Service.ChangeRole(memberId, CancellationToken.None);

        //Assert
        result.ShouldNotBeNull();
        result.Role.ShouldBe(MemberRole.Moderator);
        result.Role.ShouldNotBe(memberEntity.Role);
        result.Id.ShouldBe(memberEntity.Id);
        await MemberRepository.Received(1).Update(Arg.Any<GroupMember>(), CancellationToken.None);
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

        MemberRepository.GetFullInfoById(memberEntity.Id, CancellationToken.None).Returns(memberEntity);

        //Act
        var result = async () => await Service.ChangeRole(updateDto.Id, CancellationToken.None);

        //Assert
        await result.ShouldThrowAsync<InvalidOperationException>();
    }
}
