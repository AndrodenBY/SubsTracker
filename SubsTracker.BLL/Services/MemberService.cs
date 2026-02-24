using AutoMapper;
using DispatchR;
using SubsTracker.BLL.DTOs.User;
using SubsTracker.BLL.DTOs.User.Create;
using SubsTracker.BLL.DTOs.User.Update;
using SubsTracker.BLL.Helpers.Filters;
using SubsTracker.BLL.Interfaces;
using SubsTracker.BLL.Interfaces.Cache;
using SubsTracker.BLL.Mediator.Handlers.JoinGroup;
using SubsTracker.BLL.Mediator.Signals;
using SubsTracker.DAL.Entities;
using SubsTracker.DAL.Interfaces.Repositories;
using SubsTracker.Domain.Enums;
using SubsTracker.Domain.Exceptions;
using SubsTracker.Domain.Filter;
using SubsTracker.Domain.Pagination;

namespace SubsTracker.BLL.Services;

public class MemberService(
    IMemberRepository memberRepository,
    IMapper mapper,
    ICacheService cacheService,
    IMediator mediator) 
    : Service<MemberEntity, MemberDto, CreateMemberDto, UpdateMemberDto, MemberFilterDto>(memberRepository, mapper, cacheService),
      IMemberService
{
    public async Task<MemberDto?> GetFullInfoById(Guid id, CancellationToken cancellationToken)
    {
        var memberWithEntities = await memberRepository.GetFullInfoById(id, cancellationToken);
        return Mapper.Map<MemberDto>(memberWithEntities);
    }

    public async Task<PaginatedList<MemberDto>> GetAll(MemberFilterDto? filter, PaginationParameters? paginationParameters, CancellationToken cancellationToken)
    {
        var expression = MemberFilterHelper.CreatePredicate(filter);
        return await base.GetAll(expression, paginationParameters, cancellationToken);
    }

    public async Task<MemberDto> JoinGroup(CreateMemberDto createDto, CancellationToken cancellationToken)
    {
        return await mediator.Send(new JoinGroup(createDto), cancellationToken);
    }

    public async Task<bool> LeaveGroup(Guid groupId, Guid userId, CancellationToken cancellationToken)
    {
        var memberToDelete = await memberRepository.GetByPredicateFullInfo(member => member.GroupId == groupId && member.UserId == userId, cancellationToken)
                             ?? throw new UnknownIdentifierException($"User {userId} is not a member of group {groupId}");

        await mediator.Publish(new MemberSignals.Left(
                memberToDelete.Id, 
                memberToDelete.GroupId,
                memberToDelete.UserId,
                memberToDelete.Group.Name, 
                memberToDelete.User.Email), 
            cancellationToken);
        return await memberRepository.Delete(memberToDelete, cancellationToken);
    }

    public async Task<MemberDto> ChangeRole(Guid memberId, CancellationToken cancellationToken)
    {
        var memberToUpdate = await memberRepository.GetFullInfoById(memberId, cancellationToken)
                             ?? throw new UnknownIdentifierException($"Member with id {memberId} not found.");

        var newRole = memberToUpdate.Role switch
        {
            MemberRole.Participant => MemberRole.Moderator,
            MemberRole.Moderator => MemberRole.Participant,
            _ => throw new PolicyViolationException("Cannot modify administrator role")
        };

        var updateDto = new UpdateMemberDto { Id = memberToUpdate.Id, Role = newRole };
        Mapper.Map(updateDto, memberToUpdate);
        var updatedMember = await memberRepository.Update(memberToUpdate, cancellationToken);

        await mediator.Publish(new MemberSignals.ChangedRole(
                updatedMember.Id, 
                updatedMember.GroupId,
                updatedMember.UserId,
                updatedMember.Group.Name, 
                updatedMember.User.Email, 
                updatedMember.Role), 
            cancellationToken);
        return Mapper.Map<MemberDto>(updatedMember);
    }
}
