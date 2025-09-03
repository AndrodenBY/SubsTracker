using System.Linq.Expressions;
using AutoMapper;
using LinqKit;
using SubsTracker.BLL.DTOs.User;
using SubsTracker.BLL.DTOs.User.Create;
using SubsTracker.BLL.DTOs.User.Update;
using SubsTracker.BLL.Interfaces;
using SubsTracker.DAL.Models.User;
using SubsTracker.Domain.Enums;
using SubsTracker.Domain.Exceptions;
using SubsTracker.Domain.Filter;
using SubsTracker.Domain.Interfaces;

namespace SubsTracker.BLL.Services.User;

public class GroupMemberService(IRepository<GroupMember> repository, IMapper mapper) 
    : Service<GroupMember, GroupMemberDto, CreateGroupMemberDto, UpdateGroupMemberDto, GroupMemberFilter>(repository, mapper), IGroupMemberService
{
    public async Task<IEnumerable<GroupMemberDto>> GetAll(GroupMemberFilter? filter, CancellationToken cancellationToken)
    {
        var predicate = CreatePredicate(filter);
        
        var entities = await base.GetAll(predicate, cancellationToken);
        return entities;
    }

    private static Expression<Func<GroupMember, bool>> CreatePredicate(GroupMemberFilter filter)
    {
        var predicate = PredicateBuilder.New<GroupMember>(true);
        
        predicate = AddFilterCondition<GroupMember, Guid>(
            predicate, 
            filter.UserId, 
            member => member.UserId == filter.UserId!.Value
        );
        
        predicate = AddFilterCondition<GroupMember, Guid>(
            predicate, 
            filter.GroupId, 
            member => member.GroupId == filter.GroupId!.Value
        );
        
        predicate = AddFilterCondition<GroupMember, MemberRole>(
            predicate, 
            filter.Role, 
            member => member.Role == filter.Role!.Value
        );

        return predicate;
    }
    public async Task<bool> LeaveGroup(Guid groupId, Guid userId, GroupMemberDto memberDto, CancellationToken cancellationToken)
    {
        if (memberDto.UserId != userId)
        {
            throw new ValidationException($"Expected user with id {memberDto.UserId}, but received id {userId}");
        }
        if (memberDto.GroupId != groupId)
        {
            throw new ValidationException($"Expected group with id {memberDto.GroupId}, but received id {groupId}");
        }
        
        return await Delete(memberDto.Id, cancellationToken);
    }
}