using AutoMapper;
using DispatchR;
using SubsTracker.BLL.DTOs.User;
using SubsTracker.BLL.DTOs.User.Create;
using SubsTracker.BLL.DTOs.User.Update;
using SubsTracker.BLL.Handlers.Signals.Member;
using SubsTracker.BLL.Helpers.Filters;
using SubsTracker.BLL.Interfaces.Cache;
using SubsTracker.BLL.Interfaces.User;
using SubsTracker.DAL.Interfaces.Repositories;
using SubsTracker.DAL.Models.User;
using SubsTracker.Domain.Enums;
using SubsTracker.Domain.Exceptions;
using SubsTracker.Domain.Filter;
using SubsTracker.Domain.Pagination;

namespace SubsTracker.BLL.Services.User;

public class GroupMemberService(
    IGroupMemberRepository memberRepository,
    IMapper mapper,
    ICacheService cacheService,
    IMediator mediator) 
    : Service<GroupMember, GroupMemberDto, CreateGroupMemberDto, UpdateGroupMemberDto, GroupMemberFilterDto>(memberRepository, mapper, cacheService),
    IGroupMemberService
{
    public async Task<GroupMemberDto?> GetFullInfoById(Guid id, CancellationToken cancellationToken)
    {
        var memberWithEntities = await memberRepository.GetFullInfoById(id, cancellationToken);
        return Mapper.Map<GroupMemberDto>(memberWithEntities);
    }

    public async Task<PaginatedList<GroupMemberDto>> GetAll(GroupMemberFilterDto? filter, PaginationParameters? paginationParameters, CancellationToken cancellationToken)
    {
        var predicate = GroupMemberFilterHelper.CreatePredicate(filter);
        return await base.GetAll(predicate, paginationParameters, cancellationToken);
    }

    public async Task<GroupMemberDto> JoinGroup(CreateGroupMemberDto createDto, CancellationToken cancellationToken)
    {
        var user = memberRepository.GetFullInfoById(createDto.UserId, cancellationToken);
        if (user is null)
        {
            throw new UnknownIdentifierException($"User with id {createDto.UserId} not found");
        }

        var group = memberRepository.GetFullInfoById(createDto.GroupId, cancellationToken);
        if (group is null)
        {
            throw new UnknownIdentifierException($"Group with id {createDto.GroupId} not found");
        }

        var existingMember =
            await memberRepository.GetByPredicate(
                member => member.UserId == createDto.UserId && member.GroupId == createDto.GroupId, cancellationToken);
        if (existingMember is not null)
        {
            throw new InvalidRequestDataException("Member already exists");
        }

        return await base.Create(createDto, cancellationToken);
    }

    public async Task<bool> LeaveGroup(Guid groupId, Guid userId, CancellationToken cancellationToken)
    {
        var memberToDelete = await memberRepository.GetByPredicateFullInfo(
                                 member => member.GroupId == groupId && member.UserId == userId, cancellationToken)
                             ?? throw new UnknownIdentifierException($"User {userId} is not a member of group {groupId}");

        await mediator.Publish(new MemberLeftSignal(
            memberToDelete.Id, 
            memberToDelete.GroupId, 
            memberToDelete.Group.Name, 
            memberToDelete.User.Email), 
            cancellationToken);
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

        await mediator.Publish(new MemberChangedRoleSignal(
                updatedMember.Id, 
                updatedMember.GroupId, 
                updatedMember.Group.Name, 
                updatedMember.User.Email, 
                updatedMember.Role), 
            cancellationToken);
        return Mapper.Map<GroupMemberDto>(updatedMember);
    }
}
