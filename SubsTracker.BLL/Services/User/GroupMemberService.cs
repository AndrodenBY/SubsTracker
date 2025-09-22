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
        await EnsureExist(createDto.UserId, createDto.GroupId, cancellationToken);

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
                             ?? throw new NotFoundException($"User {userId} is not a member of group {groupId}.");

        return await base.Delete(memberToDelete.Id, cancellationToken);
    }
    
    public async Task<GroupMemberDto> ChangeRole(Guid memberId, CancellationToken cancellationToken)
    {
        var memberToUpdate = await repository.GetById(memberId, cancellationToken)
                             ?? throw new NotFoundException($"Member with id {memberId} not found.");

        var updateDto = new UpdateGroupMemberDto();
        mapper.Map(memberToUpdate, updateDto);

        if (memberToUpdate.Role == MemberRole.Admin)
        {
            throw new InvalidOperationException("Cannot modify administrator role");
        }
        
        updateDto.Role = memberToUpdate.Role switch
        {
            MemberRole.Participant => MemberRole.Moderator,
            MemberRole.Moderator => MemberRole.Participant,
            _ => memberToUpdate.Role
        };

        return await base.Update(memberToUpdate.Id, updateDto, cancellationToken);
    }
    
    private async Task EnsureExist(Guid userId, Guid groupId, CancellationToken cancellationToken)
    {
        var userTask = await userRepository.GetById(userId, cancellationToken)
                       ?? throw new NotFoundException($"User with id {userId} not found.");
        var groupTask = await groupRepository.GetById(groupId, cancellationToken)
                        ?? throw new NotFoundException($"Group with id {groupId} not found.");
    }
}
