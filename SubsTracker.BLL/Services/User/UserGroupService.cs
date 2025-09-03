using System.Linq.Expressions;
using AutoMapper;
using LinqKit;
using SubsTracker.BLL.DTOs.User;
using SubsTracker.BLL.DTOs.User.Create;
using SubsTracker.BLL.DTOs.User.Update;
using SubsTracker.BLL.Interfaces;
using SubsTracker.DAL.Interfaces;
using SubsTracker.DAL.Models.User;
using SubsTracker.Domain.Exceptions;
using SubsTracker.Domain.Filter;
using SubsTracker.Domain.Interfaces;

namespace SubsTracker.BLL.Services.User;

public class UserGroupService(
    IRepository<UserGroup> repository,
    IRepository<DAL.Models.User.User> userRepository,
    ISubscriptionRepository subscriptionRepository,
    Service<GroupMember, GroupMemberDto, CreateGroupMemberDto, UpdateGroupMemberDto, GroupMemberFilter> memberService, 
    IMapper mapper) 
    : Service<UserGroup, UserGroupDto, CreateUserGroupDto, UpdateUserGroupDto, UserGroupFilter>(repository, mapper), 
        IUserGroupService
{
    public async Task<IEnumerable<UserGroupDto>> GetAll(UserGroupFilter? filter, CancellationToken cancellationToken)
    {
        var predicate = CreatePredicate(filter);
        
        var entities = await base.GetAll(predicate, cancellationToken);
        return entities;
    }
    
    private static Expression<Func<UserGroup, bool>> CreatePredicate(UserGroupFilter filter)
    {
        var predicate = PredicateBuilder.New<UserGroup>(true);
        
        predicate = AddFilterCondition<UserGroup>(
            predicate, 
            filter.Name, 
            group => group.Name.Contains(filter.Name!, StringComparison.OrdinalIgnoreCase)
        );

        return predicate;
    }
    
    public async Task<bool> LeaveGroup(Guid groupId, Guid userId, CancellationToken cancellationToken)
    {
        var memberToDelete = await memberService.GetByPredicate(
            m => m.GroupId == groupId && m.UserId == userId, cancellationToken);

        if (memberToDelete == null) return true;
        
        return await memberService.Delete(memberToDelete.Id, cancellationToken);
    }
    
    public async Task<GroupMemberDto> JoinGroup(CreateGroupMemberDto createDto, CancellationToken cancellationToken)
    {
        await EnsureExist(createDto.UserId, createDto.GroupId, cancellationToken);
        
        var existingMember = await memberService.GetByPredicate(
            gm => gm.UserId == createDto.UserId && gm.GroupId == createDto.GroupId, cancellationToken);
        
        if(existingMember is not null)
        {
            throw new ValidationException("Member already exists");
        }
        
        return await memberService.Create(createDto, cancellationToken);
    }
    
    public async Task<UserGroupDto> ShareSubscription(Guid groupId, Guid subscriptionId, CancellationToken cancellationToken)
    {
        var isShared = await IsShared(groupId, subscriptionId, cancellationToken);
        if (isShared)
        {
            throw new ValidationException($"Subscription {subscriptionId} is already shared");
        }

        var group = await repository.GetById(groupId, cancellationToken);
        var subscription = await subscriptionRepository.GetById(subscriptionId, cancellationToken);
        
        group.SharedSubscriptions.Add(subscription);
        
        var updatedGroup = await repository.Update(group, cancellationToken);
        return mapper.Map<UserGroupDto>(updatedGroup);
    }

    public async Task<UserGroupDto> UnshareSubscription(Guid groupId, Guid subscriptionId, CancellationToken cancellationToken)
    {
        var isShared = await IsShared(groupId, subscriptionId, cancellationToken);
        if (!isShared) throw new ValidationException($"Subscription {subscriptionId} is not shared");
        
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
        var existingUser = userRepository.GetById(userId, cancellationToken);
        var existingGroup = repository.GetById(groupId, cancellationToken);
        
        await Task.WhenAll(existingUser, existingGroup);
        _ = await existingUser ?? throw new NotFoundException($"User with id {userId} not found");
        _ = await existingGroup ?? throw new NotFoundException($"Group with id {groupId} not found");
    }
}