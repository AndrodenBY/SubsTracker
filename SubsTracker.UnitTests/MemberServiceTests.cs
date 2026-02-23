using System.Linq.Expressions;
using AutoFixture;
using NSubstitute;
using Shouldly;
using SubsTracker.BLL.DTOs.User;
using SubsTracker.BLL.DTOs.User.Create;
using SubsTracker.BLL.RedisSettings;
using SubsTracker.DAL.Entities;
using SubsTracker.Domain.Enums;
using SubsTracker.Domain.Exceptions;
using SubsTracker.Domain.Filter;
using SubsTracker.Messaging.Contracts;
using SubsTracker.UnitTests.TestsBase;

namespace SubsTracker.UnitTests;

public class MemberServiceTests : MemberServiceTestBase
{
    [Fact]
public async Task LeaveGroup_WhenModelIsValid_ReturnsTrue()
{
    //Arrange
    var ct = CancellationToken.None;
    var userId = Guid.NewGuid();
    var groupId = Guid.NewGuid();

    var memberToDelete = Fixture.Build<MemberEntity>()
        .With(m => m.UserId, userId)
        .With(m => m.GroupId, groupId)
        .Create();

    MemberRepository.GetByPredicateFullInfo(Arg.Any<Expression<Func<MemberEntity, bool>>>(), Arg.Any<CancellationToken>())
        .Returns(Task.FromResult<MemberEntity?>(memberToDelete));

    MemberRepository.Delete(memberToDelete, Arg.Any<CancellationToken>())
        .Returns(Task.FromResult(true));

    //Act
    var result = await Service.LeaveGroup(groupId, userId, ct);

    //Assert
    result.ShouldBeTrue();
    await MemberRepository.Received(1)
        .GetByPredicateFullInfo(Arg.Any<Expression<Func<MemberEntity, bool>>>(), Arg.Any<CancellationToken>());
    await MemberRepository.Received(1).Delete(memberToDelete, Arg.Any<CancellationToken>());
    await MessageService.Received(1)
        .NotifyMemberLeftGroup(Arg.Is<MemberLeftGroupEvent>(e => e.GroupId == groupId), Arg.Any<CancellationToken>());
}

[Fact]
public async Task LeaveGroup_WhenNotTheMemberOfTheGroup_ThrowsNotFoundException()
{
    //Arrange
    var ct = CancellationToken.None;
    var userId = Guid.NewGuid();
    var groupId = Guid.NewGuid();

    MemberRepository.GetByPredicateFullInfo(Arg.Any<Expression<Func<MemberEntity, bool>>>(), Arg.Any<CancellationToken>())
        .Returns(Task.FromResult<MemberEntity?>(null));

    //Act
    var act = async () => await Service.LeaveGroup(groupId, userId, ct);

    //Assert
    await act.ShouldThrowAsync<UnknownIdentifierException>();
    await MemberRepository.Received(1)
        .GetByPredicateFullInfo(Arg.Any<Expression<Func<MemberEntity, bool>>>(), Arg.Any<CancellationToken>());
    await MemberRepository.DidNotReceive().Delete(Arg.Any<MemberEntity>(), Arg.Any<CancellationToken>());
}

    [Fact]
    public async Task LeaveGroup_WhenSuccessful_DeletesAndNotifies()
{
    //Arrange
    var ct = CancellationToken.None;
    var userId = Guid.NewGuid();
    var groupId = Guid.NewGuid();
    var memberId = Guid.NewGuid();

    var memberToDelete = Fixture.Build<MemberEntity>()
        .With(m => m.Id, memberId)
        .With(m => m.UserId, userId)
        .With(m => m.GroupId, groupId)
        .Create();

    MemberRepository.GetByPredicateFullInfo(Arg.Any<Expression<Func<MemberEntity, bool>>>(), Arg.Any<CancellationToken>())
        .Returns(Task.FromResult<MemberEntity?>(memberToDelete));

    MemberRepository.Delete(memberToDelete, Arg.Any<CancellationToken>())
        .Returns(Task.FromResult(true));

    var memberCacheKey = RedisKeySetter.SetCacheKey<MemberDto>(memberId);

    //Act
    var result = await Service.LeaveGroup(groupId, userId, ct);

    //Assert
    result.ShouldBeTrue();
    await MemberRepository.Received(1).Delete(memberToDelete, Arg.Any<CancellationToken>());

    await CacheAccessService.Received(1)
        .RemoveData(Arg.Is<List<string>>(keys => keys.Contains(memberCacheKey)), Arg.Any<CancellationToken>());

    await MessageService.Received(1)
        .NotifyMemberLeftGroup(
            Arg.Is<MemberLeftGroupEvent>(e => e.Id == memberId && e.GroupId == groupId),
            Arg.Any<CancellationToken>()
        );
}
    
    [Fact]
    public async Task JoinGroup_WhenValidModel_ReturnsGroupMember()
    {
        //Arrange
        var userId = Guid.NewGuid();
        var groupId = Guid.NewGuid();

        var createDto = Fixture.Build<CreateMemberDto>()
            .With(dto => dto.UserId, userId)
            .With(dto => dto.GroupId, groupId)
            .Create();
        
        var stubUserOrGroup = Fixture.Create<MemberEntity>(); 
        var createdMemberEntity = Fixture.Build<MemberEntity>()
            .With(gm => gm.UserId, userId)
            .With(gm => gm.GroupId, groupId)
            .Create();

        var createdMemberDto = Fixture.Build<MemberDto>()
            .With(dto => dto.UserId, userId)
            .With(dto => dto.GroupId, groupId)
            .Create();
        
        MemberRepository.GetFullInfoById(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(stubUserOrGroup);
        
        MemberRepository.GetByPredicate(Arg.Any<Expression<Func<MemberEntity, bool>>>(), Arg.Any<CancellationToken>())
            .Returns((MemberEntity?)null);
        
        MemberRepository.Create(Arg.Any<MemberEntity>(), Arg.Any<CancellationToken>())
            .Returns(createdMemberEntity);
        
        Mapper.Map<MemberDto>(createdMemberEntity)
            .Returns(createdMemberDto);

        //Act
        var result = await Service.JoinGroup(createDto, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.UserId.ShouldBe(userId);
        result.GroupId.ShouldBe(groupId);
    
        // Проверяем, что метод Create был вызван 1 раз
        await MemberRepository.Received(1).Create(Arg.Any<MemberEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task JoinGroup_WhenMemberAlreadyExists_ThrowsValidationException()
    {
        //Arrange
        var userId = Guid.NewGuid();
        var groupId = Guid.NewGuid();
        
        var createDto = Fixture.Build<CreateMemberDto>()
            .With(dto => dto.UserId, userId)
            .With(dto => dto.GroupId, groupId)
            .Create();
        
        var stubMember = Fixture.Create<MemberEntity>();

        MemberRepository.GetFullInfoById(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(stubMember); 
        
        var existingMemberEntity = Fixture.Build<MemberEntity>()
            .With(m => m.UserId, userId)
            .With(m => m.GroupId, groupId)
            .Create();
    
        MemberRepository.GetByPredicate(
                Arg.Any<Expression<Func<MemberEntity, bool>>>(), 
                Arg.Any<CancellationToken>())
            .Returns(existingMemberEntity);

        //Act
        var act = async () => await Service.JoinGroup(createDto, CancellationToken.None);

        //Assert
        await act.ShouldThrowAsync<InvalidRequestDataException>();
    }
    
    [Fact]
    public async Task GetFullInfoById_WhenCacheHit_ReturnsCachedDataAndSkipsRepo()
{
    //Arrange
    var ct = CancellationToken.None;
    var memberId = Fixture.Create<Guid>();
    var cacheKey = $"{memberId}:{nameof(MemberDto)}";

    var cachedDto = Fixture.Build<MemberDto>()
        .With(dto => dto.Id, memberId)
        .With(dto => dto.Role, MemberRole.Admin)
        .Create();

    CacheService.CacheDataWithLock(
        cacheKey,
        Arg.Any<TimeSpan>(),
        Arg.Any<Func<Task<MemberDto?>>>(),
        Arg.Any<CancellationToken>()
    ).Returns(Task.FromResult<MemberDto?>(cachedDto));

    //Act
    var result = await Service.GetFullInfoById(memberId, ct);

    //Assert
    result.ShouldNotBeNull();
    result.ShouldBe(cachedDto);
    result.Role.ShouldBe(MemberRole.Admin);

    await MemberRepository.DidNotReceive().GetFullInfoById(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    await CacheService.Received(1).CacheDataWithLock(
        cacheKey,
        Arg.Any<TimeSpan>(),
        Arg.Any<Func<Task<MemberDto?>>>(),
        Arg.Any<CancellationToken>()
    );
}

    [Fact]
    public async Task GetFullInfoById_WhenCacheMiss_FetchesFromRepoAndCaches()
{
    //Arrange
    var ct = CancellationToken.None;
    var memberId = Fixture.Create<Guid>();
    var cacheKey = $"{memberId}:{nameof(MemberDto)}";

    var memberEntity = Fixture.Create<MemberEntity>();
    memberEntity.Id = memberId;

    var memberDto = Fixture.Build<MemberDto>()
        .With(dto => dto.Id, memberId)
        .Create();

    MemberRepository.GetFullInfoById(memberId, Arg.Any<CancellationToken>())
        .Returns(Task.FromResult<MemberEntity?>(memberEntity));

    Mapper.Map<MemberDto>(memberEntity)
        .Returns(memberDto);

    CacheService.CacheDataWithLock(
        cacheKey,
        Arg.Any<TimeSpan>(),
        Arg.Any<Func<Task<MemberDto?>>>(),
        Arg.Any<CancellationToken>()
    ).Returns(async callInfo =>
    {
        var factory = callInfo.Arg<Func<Task<MemberDto?>>>();
        return await factory();
    });

    //Act
    var result = await Service.GetFullInfoById(memberId, ct);

    //Assert
    result.ShouldNotBeNull();
    result.ShouldBe(memberDto);
    result.Id.ShouldBe(memberId);

    await MemberRepository.Received(1).GetFullInfoById(memberId, Arg.Any<CancellationToken>());
    Mapper.Received(1).Map<MemberDto>(memberEntity);
    await CacheService.Received(1).CacheDataWithLock(
        cacheKey,
        Arg.Any<TimeSpan>(),
        Arg.Any<Func<Task<MemberDto?>>>(),
        Arg.Any<CancellationToken>()
    );
}
    
    [Fact]
    public async Task GetAll_WhenFilterIsEmpty_ReturnsAllMembers()
    {
        //Arrange
        var members = Fixture.CreateMany<MemberEntity>(3).ToList();
        var memberDtos = Fixture.CreateMany<MemberDto>(3).ToList();

        var filter = new MemberFilterDto();

        MemberRepository.GetAll(Arg.Any<Expression<Func<MemberEntity, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(members);
        Mapper.Map<List<MemberDto>>(Arg.Any<List<MemberEntity>>())
            .Returns(memberDtos);

        //Act
        var result = await Service.GetAll(filter, Arg.Any<CancellationToken>());

        //Assert
        result.ShouldNotBeNull();
        result.ShouldNotBeEmpty();
        result.Count.ShouldBe(3);
    }

    [Fact]
    public async Task GetAll_WhenNoMembers_ReturnsEmptyList()
    {
        //Arrange
        var filter = new MemberFilterDto();

        MemberRepository.GetAll(Arg.Any<Expression<Func<MemberEntity, bool>>>(), Arg.Any<CancellationToken>())
            .Returns([]);
        Mapper.Map<List<MemberDto>>(Arg.Any<List<MemberEntity>>()).Returns(new List<MemberDto>());

        //Act
        var result = await Service.GetAll(filter, Arg.Any<CancellationToken>());

        //Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetAll_WhenFilteredByName_ReturnsCorrectGroupMember()
    {
        //Arrange
        var ct = CancellationToken.None;
        var memberToFind = Fixture.Create<MemberEntity>();
        var memberDto = Fixture.Build<MemberDto>()
            .With(d => d.Id, memberToFind.Id)
            .With(d => d.Role, memberToFind.Role)
            .Create();

        var filter = new MemberFilterDto { Role = memberToFind.Role };

        MemberRepository.GetAll(Arg.Any<Expression<Func<MemberEntity, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<List<MemberEntity>>([memberToFind]));

        Mapper.Map<List<MemberDto>>(Arg.Any<List<MemberEntity>>())
            .Returns([memberDto]);

        //Act
        var result = await Service.GetAll(filter, ct);

        //Assert
        result.ShouldNotBeNull();
        result.ShouldHaveSingleItem();
        result.Single().Role.ShouldBe(memberToFind.Role);
    
        await MemberRepository.Received(1).GetAll(
            Arg.Any<Expression<Func<MemberEntity, bool>>>(),
            Arg.Any<CancellationToken>()
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

        var entities = Fixture.Build<MemberEntity>()
            .With(m => m.Role, targetRole)
            .CreateMany(2).ToList();

        var dtos = Fixture.Build<MemberDto>()
            .With(d => d.Role, targetRole)
            .CreateMany(2).ToList();

        MemberRepository.GetAll(Arg.Any<Expression<Func<MemberEntity, bool>>>(), ct)
            .Returns(entities);

        Mapper.Map<List<MemberDto>>(entities)
            .Returns(dtos);

        //Act
        var result = await Service.GetAll(filter, ct);

        //Assert
        result.Count.ShouldBe(2);
        result.All(m => m.Role == targetRole).ShouldBeTrue();
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

        MemberRepository.GetAll(Arg.Any<Expression<Func<MemberEntity, bool>>>(), ct)
            .Returns([entity]);

        Mapper.Map<List<MemberDto>>(Arg.Is<List<MemberEntity>>(l => l.Contains(entity)))
            .Returns([dto]);

        //Act
        var result = await Service.GetAll(filter, ct);

        //Assert
        result.ShouldHaveSingleItem();
        result.First().Id.ShouldBe(targetId);
    }
    
    [Fact]
    public async Task ChangeRole_WhenNotAdmin_ChangeRole()
{
    //Arrange
    var ct = CancellationToken.None;
    var memberId = Guid.NewGuid();
    var memberEntity = Fixture.Build<MemberEntity>()
        .With(member => member.Id, memberId)
        .With(member => member.Role, MemberRole.Participant)
        .Create();

    var updatedMemberEntity = Fixture.Build<MemberEntity>()
        .With(member => member.Id, memberId)
        .With(member => member.Role, MemberRole.Moderator)
        .Create();

    var memberDto = Fixture.Build<MemberDto>()
        .With(dto => dto.Id, memberId)
        .With(dto => dto.Role, MemberRole.Moderator)
        .Create();

    MemberRepository.GetFullInfoById(memberId, Arg.Any<CancellationToken>())
        .Returns(Task.FromResult<MemberEntity?>(memberEntity));

    MemberRepository.Update(memberEntity, Arg.Any<CancellationToken>())
        .Returns(Task.FromResult(updatedMemberEntity));

    Mapper.Map<MemberDto>(updatedMemberEntity)
        .Returns(memberDto);

    //Act
    var result = await Service.ChangeRole(memberId, ct);

    //Assert
    result.ShouldNotBeNull();
    result.Role.ShouldBe(MemberRole.Moderator);
    result.Id.ShouldBe(memberEntity.Id);
    await MemberRepository.Received(1).Update(Arg.Any<MemberEntity>(), Arg.Any<CancellationToken>());
    await MessageService.Received(1)
        .NotifyMemberChangedRole(Arg.Is<MemberChangedRoleEvent>(e => e.Id == memberId), Arg.Any<CancellationToken>());
}

    [Fact]
    public async Task ChangeRole_WhenAdmin_ThrowsPolicyViolationException()
{
    //Arrange
    var ct = CancellationToken.None;
    var memberEntity = Fixture.Build<MemberEntity>()
        .With(member => member.Role, MemberRole.Admin)
        .Create();

    MemberRepository.GetFullInfoById(memberEntity.Id, Arg.Any<CancellationToken>())
        .Returns(Task.FromResult<MemberEntity?>(memberEntity));

    //Act
    var act = async () => await Service.ChangeRole(memberEntity.Id, ct);

    //Assert
    await act.ShouldThrowAsync<PolicyViolationException>();
}
}
