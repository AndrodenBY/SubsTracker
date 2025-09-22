using AutoMapper;
using SubsTracker.BLL.DTOs.User;
using SubsTracker.BLL.DTOs.User.Create;
using SubsTracker.BLL.DTOs.User.Update;
using SubsTracker.BLL.Helpers.Filters;
using SubsTracker.BLL.Interfaces;
using SubsTracker.BLL.Interfaces.User;
using SubsTracker.DAL.Interfaces.Repositories;
using SubsTracker.DAL.Models.User;
using SubsTracker.Domain.Enums;
using SubsTracker.Domain.Exceptions;
using SubsTracker.Domain.Filter;
using InvalidOperationException = SubsTracker.Domain.Exceptions.InvalidOperationException;

namespace SubsTracker.BLL.Services.User;

public class UserGroupService(
    IRepository<UserGroup> repository,
    IRepository<DAL.Models.User.User> userRepository,
    IRepository<GroupMember> memberRepository,
    ISubscriptionRepository subscriptionRepository,
    IService<GroupMember, GroupMemberDto, CreateGroupMemberDto, UpdateGroupMemberDto, GroupMemberFilterDto> memberService,
    IMapper mapper
    ) : Service<UserGroup, UserGroupDto, CreateUserGroupDto, UpdateUserGroupDto, UserGroupFilterDto>(repository, mapper),
    IUserGroupService
{
    public async Task<List<UserGroupDto>> GetAll(UserGroupFilterDto? filter, CancellationToken cancellationToken)
    {
        var predicate = UserGroupFilterHelper.CreatePredicate(filter);

        var entities = await base.GetAll(predicate, cancellationToken);
        return entities;
    }

    public async Task<List<GroupMemberDto>> GetAll(GroupMemberFilterDto? filter, CancellationToken cancellationToken)
    {
        var predicate = GroupMemberFilterHelper.CreatePredicate(filter);

        var entities = await memberService.GetAll(predicate, cancellationToken);
        return entities;
    }
    
    public async Task<UserGroupDto> Create(Guid userId, CreateUserGroupDto createDto, CancellationToken cancellationToken)
    {
        var existingUser = await userRepository.GetById(userId, cancellationToken)
            ?? throw new ValidationException($"User with id {userId} does not exist");
        createDto.UserId = userId;
        
        var createdGroup = await base.Create(createDto, cancellationToken);
        
        var createMemberDto = new CreateGroupMemberDto
        {
            UserId = existingUser.Id,
            GroupId = createdGroup.Id,
            Role = MemberRole.Admin
        };
        await memberService.Create(createMemberDto, cancellationToken);
        
        return createdGroup;
    }

    public async Task<GroupMemberDto> JoinGroup(CreateGroupMemberDto createDto, CancellationToken cancellationToken)
    {
        await EnsureExist(createDto.UserId, createDto.GroupId, cancellationToken);

        var existingMember = await memberService.GetByPredicate(
            gm => gm.UserId == createDto.UserId && gm.GroupId == createDto.GroupId, cancellationToken);

        if (existingMember is not null)
        {
            throw new ValidationException("Member already exists");
        }

        return await memberService.Create(createDto, cancellationToken);
    }

    public async Task<bool> LeaveGroup(Guid groupId, Guid userId, CancellationToken cancellationToken)
    {
        var memberToDelete = await memberService.GetByPredicate(
            member => member.GroupId == groupId && member.UserId == userId, cancellationToken)
            ?? throw new NotFoundException($"User {userId} is not a member of group {groupId}.");

        return await memberService.Delete(memberToDelete.Id, cancellationToken);
    }

    public async Task<GroupMemberDto> MakeModerator(Guid memberId, CancellationToken cancellationToken)
    {
        var memberToUpdate = await memberRepository.GetById(memberId, cancellationToken)
                             ?? throw new NotFoundException($"Member with id {memberId} not found.");

        var updateDto = new UpdateGroupMemberDto();
        mapper.Map(memberToUpdate, updateDto);
        updateDto.Role = MemberRole.Moderator;

        return await memberService.Update(memberToUpdate.Id, updateDto, cancellationToken);
    }

    public async Task<UserGroupDto> ShareSubscription(Guid groupId, Guid subscriptionId, CancellationToken cancellationToken)
    {
        var group = await repository.GetByPredicate(
            group => group.Id == groupId && group.SharedSubscriptions.Any(sub => sub.Id == subscriptionId),
            cancellationToken);

        if (group is not null)
        {
            throw new InvalidOperationException($"Subscription with id {subscriptionId} is already shared with group {groupId}.");
        }

        group = await repository.GetById(groupId, cancellationToken)
                ?? throw new NotFoundException($"Group with id {groupId} not found.");

        var subscription = await subscriptionRepository.GetById(subscriptionId, cancellationToken)
                           ?? throw new NotFoundException($"Subscription with id {subscriptionId} not found.");

        group.SharedSubscriptions.Add(subscription);

        var updatedGroup = await repository.Update(group, cancellationToken);
        return mapper.Map<UserGroupDto>(updatedGroup);
    }

    public async Task<UserGroupDto> UnshareSubscription(Guid groupId, Guid subscriptionId, CancellationToken cancellationToken)
    {
        var isShared = await IsShared(groupId, subscriptionId, cancellationToken);
        if (!isShared)
        {
            throw new ValidationException($"Subscription {subscriptionId} is not shared");
        }

        var subscription = await subscriptionRepository.GetById(groupId, cancellationToken)
            ?? throw new NotFoundException($"Subscription with id {subscriptionId} not found");

        var group = await repository.GetById(groupId, cancellationToken);
        group.SharedSubscriptions.Remove(subscription);

        var updatedGroup = await repository.Update(group, cancellationToken);
        return mapper.Map<UserGroupDto>(updatedGroup);
    }

    private async Task<bool> IsShared(Guid groupId, Guid subscriptionId, CancellationToken cancellationToken)
    {
        var group = await repository.GetByPredicate(
            g => g.Id == groupId && g.SharedSubscriptions.Any(s => s.Id == subscriptionId), cancellationToken);
        return group is not null;
    }

    private async Task EnsureExist(Guid userId, Guid groupId, CancellationToken cancellationToken)
    {
        var userTask = userRepository.GetById(userId, cancellationToken);
        var groupTask = repository.GetById(groupId, cancellationToken);

        _ = await userTask ?? throw new NotFoundException($"User with id {userId} not found.");
        _ = await groupTask ?? throw new NotFoundException($"Group with id {groupId} not found.");
    }
}
