using AutoMapper;
using SubsTracker.BLL.DTOs.User;
using SubsTracker.BLL.DTOs.User.Create;
using SubsTracker.BLL.DTOs.User.Update;
using SubsTracker.BLL.Helpers.Filters;
using SubsTracker.DAL.Interfaces.Repositories;
using SubsTracker.BLL.Interfaces.User;
using SubsTracker.DAL.Models.User;
using SubsTracker.Domain.Enums;
using SubsTracker.Domain.Exceptions;
using SubsTracker.Domain.Filter;
using InvalidOperationException = SubsTracker.Domain.Exceptions.InvalidOperationException;

namespace SubsTracker.BLL.Services.User;

public class GroupMemberService(
    IRepository<GroupMember> repository, 
    IRepository<UserGroup> groupRepository, 
    IRepository<DAL.Models.User.User> userRepository, 
    IMapper mapper
    ) : Service<GroupMember, GroupMemberDto, CreateGroupMemberDto, UpdateGroupMemberDto, GroupMemberFilterDto>(repository, mapper), 
    IGroupMemberService
{
    public async Task<List<GroupMemberDto>> GetAll(GroupMemberFilterDto? filter, CancellationToken cancellationToken)
    {
        var predicate = GroupMemberFilterHelper.CreatePredicate(filter);

        var entities = await base.GetAll(predicate, cancellationToken);
        return entities;
    }
    
    public async Task<GroupMemberDto> JoinGroup(CreateGroupMemberDto createDto, CancellationToken cancellationToken)
    {
        var user = repository.GetById(createDto.UserId, default)
            ?? throw new NotFoundException($"User with id {createDto.UserId} not found");
        
        var group = repository.GetById(createDto.GroupId, default)
            ?? throw new NotFoundException($"Group with id {createDto.GroupId} not found");

        var existingMember = await repository.GetByPredicate(
            gm => gm.UserId == createDto.UserId && gm.GroupId == createDto.GroupId, cancellationToken);

        if (existingMember is not null)
        {
            throw new ValidationException("Member already exists");
        }

        return await base.Create(createDto, cancellationToken);
    }
    
    public async Task<bool> LeaveGroup(Guid groupId, Guid userId, CancellationToken cancellationToken)
    {
        var memberToDelete = await repository.GetByPredicate(
                                 member => member.GroupId == groupId && member.UserId == userId, cancellationToken)
                             ?? throw new NotFoundException($"User {userId} is not a member of group {groupId}");

        return await base.Delete(memberToDelete.Id, cancellationToken);
    }
    
    public async Task<GroupMemberDto> ChangeRole(Guid memberId, CancellationToken cancellationToken)
    {
        var memberToUpdate = await repository.GetById(memberId, cancellationToken)
                             ?? throw new NotFoundException($"Member with id {memberId} not found.");
        
        var newRole = memberToUpdate.Role switch
        {
            MemberRole.Participant => MemberRole.Moderator,
            MemberRole.Moderator => MemberRole.Participant,
            _ => throw new InvalidOperationException("Cannot modify administrator role")
        };
        
        var updateDto = new UpdateGroupMemberDto { Id = memberToUpdate.Id, Role = newRole };
        
        return await base.Update(memberToUpdate.Id, updateDto, cancellationToken);
    }
}
