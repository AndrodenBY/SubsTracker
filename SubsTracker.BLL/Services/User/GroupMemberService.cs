using AutoMapper;
using SubsTracker.BLL.DTOs.User;
using SubsTracker.BLL.DTOs.User.Create;
using SubsTracker.BLL.DTOs.User.Update;
using SubsTracker.BLL.Helpers.Filters;
using SubsTracker.BLL.Helpers.Notifications;
using SubsTracker.BLL.Interfaces.Cache;
using SubsTracker.BLL.Interfaces.User;
using SubsTracker.BLL.RedisSettings;
using SubsTracker.DAL.Interfaces.Repositories;
using SubsTracker.DAL.Models.User;
using SubsTracker.Domain.Enums;
using SubsTracker.Domain.Exceptions;
using SubsTracker.Domain.Filter;
using SubsTracker.Messaging.Interfaces;

namespace SubsTracker.BLL.Services.User;

public class GroupMemberService(
    IGroupMemberRepository memberRepository,
    IMessageService messageService,
    IMapper mapper,
    ICacheService cacheService,
    ICacheAccessService cacheAccessService
) : Service<GroupMember, GroupMemberDto, CreateGroupMemberDto, UpdateGroupMemberDto, GroupMemberFilterDto>(
        memberRepository, mapper, cacheService),
    IGroupMemberService
{
    public async Task<GroupMemberDto?> GetFullInfoById(Guid id, CancellationToken cancellationToken)
    {
        var cacheKey = RedisKeySetter.SetCacheKey<GroupMemberDto>(id);
        return await CacheService.CacheDataWithLock(cacheKey, RedisConstants.ExpirationTime, GetGroupMember,
            cancellationToken);

        async Task<GroupMemberDto?> GetGroupMember()
        {
            var memberWithEntities = await memberRepository.GetFullInfoById(id, cancellationToken);
            return Mapper.Map<GroupMemberDto>(memberWithEntities);
        }
    }

    public async Task<List<GroupMemberDto>> GetAll(GroupMemberFilterDto? filter, CancellationToken cancellationToken)
    {
        var predicate = GroupMemberFilterHelper.CreatePredicate(filter);
        return await base.GetAll(predicate, cancellationToken);
    }

    public async Task<GroupMemberDto> JoinGroup(CreateGroupMemberDto createDto, CancellationToken cancellationToken)
    {
        var user = memberRepository.GetFullInfoById(createDto.UserId, cancellationToken);
        if (user is null) throw new UnknownIdentifierException($"User with id {createDto.UserId} not found");

        var group = memberRepository.GetFullInfoById(createDto.GroupId, cancellationToken);
        if (group is null) throw new UnknownIdentifierException($"Group with id {createDto.GroupId} not found");

        var existingMember =
            await memberRepository.GetByPredicate(
                gm => gm.UserId == createDto.UserId && gm.GroupId == createDto.GroupId, cancellationToken);
        if (existingMember is not null) throw new InvalidRequestDataException("Member already exists");

        return await base.Create(createDto, cancellationToken);
    }

    public async Task<bool> LeaveGroup(Guid groupId, Guid userId, CancellationToken cancellationToken)
    {
        var memberToDelete = await memberRepository.GetByPredicateFullInfo(
                                 member => member.GroupId == groupId && member.UserId == userId, cancellationToken)
                             ?? throw new UnknownIdentifierException($"User {userId} is not a member of group {groupId}");

        var memberLeftEvent = GroupMemberNotificationHelper.CreateMemberLeftGroupEvent(memberToDelete);

        await cacheAccessService.RemoveData([RedisKeySetter.SetCacheKey<GroupMemberDto>(memberToDelete.Id)],
            cancellationToken);
        await messageService.NotifyMemberLeftGroup(memberLeftEvent, cancellationToken);
        return await memberRepository.Delete(memberToDelete, cancellationToken);
    }

    public async Task<GroupMemberDto> ChangeRole(Guid memberId, CancellationToken cancellationToken)
    {
        var memberToUpdate = await memberRepository.GetFullInfoById(memberId, cancellationToken)
                             ?? throw new UnknownIdentifierException($"Member with id {memberId} not found.");

        var newRole = memberToUpdate.Role switch
        {
            MemberRole.Participant => MemberRole.Moderator,
            MemberRole.Moderator => MemberRole.Participant,
            _ => throw new PolicyViolationException("Cannot modify administrator role")
        };

        var updateDto = new UpdateGroupMemberDto { Id = memberToUpdate.Id, Role = newRole };
        Mapper.Map(updateDto, memberToUpdate);
        var updatedMember = await memberRepository.Update(memberToUpdate, cancellationToken);

        var memberChangedRoleEvent = GroupMemberNotificationHelper.CreateMemberChangedRoleEvent(updatedMember);
        await messageService.NotifyMemberChangedRole(memberChangedRoleEvent, cancellationToken);
        return Mapper.Map<GroupMemberDto>(updatedMember);
    }
}
