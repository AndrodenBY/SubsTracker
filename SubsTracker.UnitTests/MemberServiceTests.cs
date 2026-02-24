using System.Linq.Expressions;
using AutoFixture;
using NSubstitute;
using Shouldly;
using SubsTracker.BLL.DTOs.User;
using SubsTracker.BLL.DTOs.User.Create;
using SubsTracker.BLL.DTOs.User.Update;
using SubsTracker.BLL.Helpers.Policy;
using SubsTracker.BLL.Mediator.Handlers.JoinGroup;
using SubsTracker.BLL.Mediator.Signals;
using SubsTracker.DAL.Entities;
using SubsTracker.Domain.Enums;
using SubsTracker.Domain.Exceptions;
using SubsTracker.Domain.Filter;
using SubsTracker.Domain.Pagination;
using SubsTracker.UnitTests.TestsBase;

namespace SubsTracker.UnitTests;

public class MemberServiceTests : MemberServiceTestBase
{
    [Fact]
    public async Task LeaveGroup_WhenModelIsValid_ReturnsTrueAndPublishesSignal()
    {
        //Arrange
        var ct = CancellationToken.None;
        var userId = Guid.NewGuid();
        var groupId = Guid.NewGuid();
        var groupName = "Test Group";
        var userEmail = "test@example.com";
        
        var memberToDelete = Fixture.Build<MemberEntity>()
            .With(m => m.UserId, userId)
            .With(m => m.GroupId, groupId)
            .With(m => m.Group, new GroupEntity { Name = groupName })
            .With(m => m.User, new UserEntity { Email = userEmail })
            .Create();

        MemberRepository.GetByPredicateFullInfo(Arg.Any<Expression<Func<MemberEntity, bool>>>(), ct)
            .Returns(memberToDelete);

        MemberRepository.Delete(memberToDelete, ct)
            .Returns(true);

        //Act
        var result = await Service.LeaveGroup(groupId, userId, ct);

        //Assert
        result.ShouldBeTrue();

        await MemberRepository.Received(1).Delete(memberToDelete, ct);
        await Mediator.Received(1).Publish(
            Arg.Is<MemberSignals.Left>(s => 
                s.MemberId == memberToDelete.Id &&
                s.GroupId == groupId &&
                s.UserId == userId &&
                s.GroupName == groupName &&
                s.UserEmail == userEmail), 
            ct);
    }

    [Fact]
    public async Task LeaveGroup_WhenNotTheMemberOfTheGroup_ThrowsUnknownIdentifierException()
    {
        //Arrange
        var ct = CancellationToken.None;
        var userId = Guid.NewGuid();
        var groupId = Guid.NewGuid();
        
        MemberRepository.GetByPredicateFullInfo(Arg.Any<Expression<Func<MemberEntity, bool>>>(), ct)
            .Returns(Task.FromResult<MemberEntity?>(null));

        //Act
        var act = async () => await Service.LeaveGroup(groupId, userId, ct);

        //Assert
        await act.ShouldThrowAsync<UnknownIdentifierException>();
        await MemberRepository.Received(1)
            .GetByPredicateFullInfo(Arg.Any<Expression<Func<MemberEntity, bool>>>(), ct);
        await MemberRepository.DidNotReceive().Delete(Arg.Any<MemberEntity>(), ct);
        await Mediator.DidNotReceive().Publish(Arg.Any<MemberSignals.Left>(), ct);
    }

    [Fact]
    public async Task LeaveGroup_WhenSuccessful_DeletesAndPublishesSignal()
    {
        //Arrange
        var ct = CancellationToken.None;
        var userId = Guid.NewGuid();
        var groupId = Guid.NewGuid();
        var groupName = "Subscribers Group";
        var userEmail = "user@test.com";
        
        var memberToDelete = Fixture.Build<MemberEntity>()
            .With(m => m.UserId, userId)
            .With(m => m.GroupId, groupId)
            .With(m => m.Group, new GroupEntity { Name = groupName })
            .With(m => m.User, new UserEntity { Email = userEmail })
            .Create();

        MemberRepository.GetByPredicateFullInfo(Arg.Any<Expression<Func<MemberEntity, bool>>>(), ct)
            .Returns(memberToDelete);

        MemberRepository.Delete(memberToDelete, ct)
            .Returns(true);

        //Act
        var result = await Service.LeaveGroup(groupId, userId, ct);

        //Assert
        result.ShouldBeTrue();
        
        await MemberRepository.Received(1).Delete(memberToDelete, ct);
        await Mediator.Received(1).Publish(
            Arg.Is<MemberSignals.Left>(s => 
                s.MemberId == memberToDelete.Id &&
                s.GroupId == groupId &&
                s.UserId == userId &&
                s.GroupName == groupName &&
                s.UserEmail == userEmail), 
            ct);
    }
    
    [Fact]
    public async Task JoinGroupHandle_WhenValidRequest_ShouldCreateMemberAndPublishSignal()
    {
        //Arrange
        var ct = CancellationToken.None;
        var createDto = Fixture.Create<CreateMemberDto>();
        var memberEntity = Fixture.Create<MemberEntity>();
        var memberDto = Fixture.Build<MemberDto>()
            .With(x => x.UserId, createDto.UserId)
            .With(x => x.GroupId, createDto.GroupId)
            .Create();

        Mapper.Map<MemberEntity>(createDto).Returns(memberEntity);
        MemberRepository.Create(memberEntity, ct).Returns(memberEntity);
        Mapper.Map<MemberDto>(memberEntity).Returns(memberDto);

        //Act
        var handler = new JoinGroupHandler(MemberRepository, MemberPolicyChecker, Mediator, Mapper);
        var result = await handler.Handle(new JoinGroup(createDto), ct);

        //Assert
        result.ShouldNotBeNull();
        
        await MemberPolicyChecker.Received(1).EnsureCanJoinGroup(createDto, ct);
        await Mediator.Received(1).Publish(
            Arg.Is<MemberSignals.Joined>(s => s.UserId == createDto.UserId && s.GroupId == createDto.GroupId), 
            ct);
    }

    [Fact]
    public async Task EnsureCanJoinGroup_WhenMemberAlreadyExists_ThrowsPolicyViolationException()
    {
        //Arrange
        var ct = CancellationToken.None;
        var dto = Fixture.Create<CreateMemberDto>();
        var existingMember = Fixture.Create<MemberEntity>();
        
        UserRepository.GetById(dto.UserId, ct).Returns(new UserEntity());
        GroupRepository.GetById(dto.GroupId, ct).Returns(new GroupEntity());
        
        MemberRepository.GetByPredicate(Arg.Any<Expression<Func<MemberEntity, bool>>>(), ct)
            .Returns(existingMember);

        var policy = new MemberPolicyChecker(UserRepository, GroupRepository, MemberRepository);

        //Act
        var act = async () => await policy.EnsureCanJoinGroup(dto, ct);

        //Assert
        await act.ShouldThrowAsync<PolicyViolationException>();
    }
    
    [Fact]
    public async Task GetFullInfoById_WhenMemberExists_ReturnsMappedMemberDto()
    {  
        //Arrange
        var ct = CancellationToken.None;
        var memberId = Guid.NewGuid();
        
        var memberEntity = Fixture.Build<MemberEntity>()
            .With(m => m.Id, memberId)
            .Create();
        
        var expectedDto = Fixture.Build<MemberDto>()
            .With(dto => dto.Id, memberId)
            .With(dto => dto.Role, MemberRole.Admin)
            .Create();
        
        MemberRepository.GetFullInfoById(memberId, ct)
            .Returns(memberEntity);

        Mapper.Map<MemberDto>(memberEntity)
            .Returns(expectedDto);

        //Act
        var result = await Service.GetFullInfoById(memberId, ct);

        //Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(memberId);
        result.Role.ShouldBe(MemberRole.Admin);

        await MemberRepository.Received(1).GetFullInfoById(memberId, ct);
        Mapper.Received(1).Map<MemberDto>(memberEntity);
    }

    [Fact]
    public async Task GetFullInfoById_WhenMemberExists_FetchesFromRepoAndMaps()
    {
        //Arrange
        var ct = CancellationToken.None;
        var memberId = Guid.NewGuid();

        var memberEntity = Fixture.Build<MemberEntity>()
            .With(m => m.Id, memberId)
            .Create();

        var memberDto = Fixture.Build<MemberDto>()
            .With(dto => dto.Id, memberId)
            .Create();
        
        MemberRepository.GetFullInfoById(memberId, ct)
            .Returns(memberEntity);
        
        Mapper.Map<MemberDto>(memberEntity)
            .Returns(memberDto);

        //Act
        var result = await Service.GetFullInfoById(memberId, ct);

        //Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(memberId);

        await MemberRepository.Received(1).GetFullInfoById(memberId, ct);
        Mapper.Received(1).Map<MemberDto>(memberEntity);
    }
    
    [Fact]
    public async Task GetAll_WhenFilterIsEmpty_ReturnsAllMembers()
    {
        //Arrange
        var ct = CancellationToken.None;
        var filter = new MemberFilterDto();

        List<MemberEntity> members = [.. Fixture.CreateMany<MemberEntity>(3)];
        List<MemberDto> memberDtos = [.. Fixture.CreateMany<MemberDto>(3)];
        
        var pagedList = new PaginatedList<MemberEntity>(members, 1, 10, 3);
    
        MemberRepository.GetAll(
                Arg.Any<Expression<Func<MemberEntity, bool>>>(), 
                Arg.Any<PaginationParameters?>(), 
                Arg.Is(ct))
            .Returns(pagedList);
        
        Mapper.Map<MemberDto>(Arg.Any<MemberEntity>())
            .Returns(memberDtos[0], memberDtos[1], memberDtos[2]);
    
        //Act
        var result = await Service.GetAll(filter, null, ct);

        //Assert
        result.ShouldNotBeNull();
        result.Items.Count.ShouldBe(3);
        result.Items.ShouldBe(memberDtos);

        await MemberRepository.Received(1).GetAll(
            Arg.Any<Expression<Func<MemberEntity, bool>>>(),
            Arg.Any<PaginationParameters?>(),
            ct
        );
    }

    [Fact]
    public async Task GetAll_WhenNoMembers_ReturnsEmptyList()
    {
        //Arrange
        var ct = CancellationToken.None;
        var filter = new MemberFilterDto();
        
        var emptyPagedList = new PaginatedList<MemberEntity>([], 1, 10, 0);
        
        MemberRepository.GetAll(
                Arg.Any<Expression<Func<MemberEntity, bool>>>(), 
                Arg.Any<PaginationParameters?>(), 
                Arg.Is(ct))
            .Returns(emptyPagedList);

        Mapper.Map<List<MemberDto>>(Arg.Any<List<MemberEntity>>()).Returns([]);

        //Act
        var result = await Service.GetAll(filter, null, ct);

        //Assert
        result.Items.ShouldBeEmpty();
    
        await MemberRepository.Received(1).GetAll(
            Arg.Any<Expression<Func<MemberEntity, bool>>>(),
            Arg.Any<PaginationParameters?>(),
            ct
        );
    }

    [Fact]
    public async Task GetAll_WhenFilteredByRole_ReturnsCorrectGroupMember()
    {
        //Arrange
        var ct = CancellationToken.None;
        var memberToFind = Fixture.Create<MemberEntity>();
        var memberDto = Fixture.Build<MemberDto>()
            .With(d => d.Id, memberToFind.Id)
            .With(d => d.Role, memberToFind.Role)
            .Create();

        var filter = new MemberFilterDto { Role = memberToFind.Role };
        var pagedList = new PaginatedList<MemberEntity>([memberToFind], 1, 10, 1);
    
        MemberRepository.GetAll(
                Arg.Any<Expression<Func<MemberEntity, bool>>>(), 
                Arg.Any<PaginationParameters?>(), 
                Arg.Is(ct))
            .Returns(pagedList);
        
        Mapper.Map<MemberDto>(Arg.Is<MemberEntity>(e => e.Id == memberToFind.Id))
            .Returns(memberDto);

        //Act
        var result = await Service.GetAll(filter, null, ct);
        
        //Assert
        result.ShouldNotBeNull();
        result.Items.ShouldHaveSingleItem();
        result.Items[0].Role.ShouldBe(memberToFind.Role);

        await MemberRepository.Received(1).GetAll(
            Arg.Any<Expression<Func<MemberEntity, bool>>>(),
            Arg.Any<PaginationParameters?>(),
            ct
        );
    }
    
    [Theory]
    [InlineData(MemberRole.Admin)]
    [InlineData(MemberRole.Participant)]
    [InlineData(MemberRole.Moderator)]
    public async Task GetAll_WhenFilteredByRole_ReturnsMatchingMembers(MemberRole targetRole)
    {
        //Arrange
        var ct = CancellationToken.None;
        var filter = new MemberFilterDto { Role = targetRole };

        List<MemberEntity> entities = [.. Fixture.Build<MemberEntity>()
            .With(m => m.Role, targetRole)
            .CreateMany(2)];

        List<MemberDto> dtos = [.. Fixture.Build<MemberDto>()
            .With(d => d.Role, targetRole)
            .CreateMany(2)];
    
        var pagedList = new PaginatedList<MemberEntity>(entities, 1, 10, 2);
    
        MemberRepository.GetAll(
                Arg.Any<Expression<Func<MemberEntity, bool>>>(), 
                Arg.Any<PaginationParameters?>(), 
                Arg.Is(ct))
            .Returns(pagedList);
        
        Mapper.Map<MemberDto>(Arg.Any<MemberEntity>())
            .Returns(dtos[0], dtos[1]);

        //Act
        var result = await Service.GetAll(filter, null, ct);

        //Assert
        result.Items.Count.ShouldBe(2);
        result.Items.All(m => m.Role == targetRole).ShouldBeTrue();

        await MemberRepository.Received(1).GetAll(
            Arg.Any<Expression<Func<MemberEntity, bool>>>(),
            Arg.Any<PaginationParameters?>(),
            ct
        );
    }
    
    [Fact]
    public async Task GetAll_WhenFilteredById_ReturnsSpecificMember()
    {
        //Arrange
        var ct = CancellationToken.None;
        var targetId = Guid.NewGuid();
        var filter = new MemberFilterDto { Id = targetId };

        var entity = Fixture.Build<MemberEntity>().With(m => m.Id, targetId).Create();
        var dto = Fixture.Build<MemberDto>().With(d => d.Id, targetId).Create();
    
        var pagedList = new PaginatedList<MemberEntity>([entity], 1, 10, 1);
    
        MemberRepository.GetAll(
                Arg.Any<Expression<Func<MemberEntity, bool>>>(), 
                Arg.Any<PaginationParameters?>(), 
                Arg.Is(ct))
            .Returns(pagedList);
        
        Mapper.Map<MemberDto>(Arg.Is<MemberEntity>(e => e.Id == targetId))
            .Returns(dto);

        //Act
        var result = await Service.GetAll(filter, null, ct);

        //Assert
        result.Items.ShouldHaveSingleItem();
        result.Items[0].Id.ShouldBe(targetId);

        await MemberRepository.Received(1).GetAll(
            Arg.Any<Expression<Func<MemberEntity, bool>>>(),
            Arg.Any<PaginationParameters?>(),
            ct
        );
    }
    
    [Fact]
    public async Task ChangeRole_WhenNotAdmin_ChangeRoleAndPublishesSignal()
    {
        //Arrange
        var ct = CancellationToken.None;
        var memberId = Guid.NewGuid();
        var groupName = "Subscribers Group";
        var userEmail = "member@test.com";
        
        var memberEntity = Fixture.Build<MemberEntity>()
            .With(m => m.Id, memberId)
            .With(m => m.Role, MemberRole.Participant)
            .With(m => m.Group, new GroupEntity { Name = groupName })
            .With(m => m.User, new UserEntity { Email = userEmail })
            .Create();

        var memberDto = Fixture.Build<MemberDto>()
            .With(dto => dto.Id, memberId)
            .With(dto => dto.Role, MemberRole.Moderator)
            .Create();

        MemberRepository.GetFullInfoById(memberId, ct)
            .Returns(memberEntity);
        
        MemberRepository.Update(memberEntity, ct)
            .Returns(x => (MemberEntity)x[0]);
        
        Mapper.When(x => x.Map(Arg.Any<UpdateMemberDto>(), Arg.Any<MemberEntity>()))
            .Do(x => 
            {
                var source = x.Arg<UpdateMemberDto>();
                var target = x.Arg<MemberEntity>();
                target.Role = source.Role ?? MemberRole.Participant;
            });

        Mapper.Map<MemberDto>(Arg.Is<MemberEntity>(m => m.Id == memberId))
            .Returns(memberDto);
        
        //Act
        var result = await Service.ChangeRole(memberId, ct);

        //Assert
        result.ShouldNotBeNull();
        result.Role.ShouldBe(MemberRole.Moderator);
    
        await MemberRepository.Received(1).Update(memberEntity, ct);
        await Mediator.Received(1).Publish(
            Arg.Is<MemberSignals.ChangedRole>(s => 
                s.MemberId == memberId &&
                s.GroupId == memberEntity.GroupId &&
                s.UserId == memberEntity.UserId &&
                s.GroupName == groupName &&
                s.UserEmail == userEmail &&
                s.NewRole == MemberRole.Moderator), 
            ct);
        
        await Mediator.Received(1).Publish(
            Arg.Is<MemberSignals.ChangedRole>(s => s.NewRole == MemberRole.Moderator), 
            ct);
    }

    [Fact]
    public async Task ChangeRole_WhenAdmin_ThrowsPolicyViolationException()
    {
        //Arrange
        var ct = CancellationToken.None;
        var memberId = Guid.NewGuid();
        
        var memberEntity = Fixture.Build<MemberEntity>()
            .With(m => m.Id, memberId)
            .With(m => m.Role, MemberRole.Admin) 
            .Create();

        MemberRepository.GetFullInfoById(memberId, ct)
            .Returns(memberEntity);

        //Act
        var act = async () => await Service.ChangeRole(memberId, ct);

        //Assert
        await act.ShouldThrowAsync<PolicyViolationException>();
        await MemberRepository.DidNotReceive().Update(Arg.Any<MemberEntity>(), ct);
        await Mediator.DidNotReceive().Publish(Arg.Any<MemberSignals.ChangedRole>(), ct);
    }
}
