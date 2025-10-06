using AutoMapper;
using SubsTracker.BLL.DTOs.User;
using SubsTracker.BLL.DTOs.User.Create;
using SubsTracker.BLL.DTOs.User.Update;
using SubsTracker.BLL.Helpers.Filters;
using SubsTracker.BLL.Interfaces;
using SubsTracker.BLL.Interfaces.User;
using SubsTracker.DAL.Interfaces.Repositories;
using SubsTracker.DAL.Models.User;
using SubsTracker.DAL.Repository;
using SubsTracker.Domain.Enums;
using SubsTracker.Domain.Exceptions;
using SubsTracker.Domain.Filter;
using InvalidOperationException = SubsTracker.Domain.Exceptions.InvalidOperationException;

namespace SubsTracker.BLL.Services.User;

public class UserGroupService(
    IUserGroupRepository userGroupRepository,
    IRepository<DAL.Models.User.User> userRepository,
    ISubscriptionRepository subscriptionRepository,
    IGroupMemberService memberService,
    IMapper mapper
    ) : Service<UserGroup, UserGroupDto, CreateUserGroupDto, UpdateUserGroupDto, UserGroupFilterDto>(userGroupRepository, mapper),
    IUserGroupService
{
    public async Task<UserGroupDto?> GetById(Guid id, CancellationToken cancellationToken)
    {
        var groupWithConnectedEntities = await userGroupRepository.GetById(id, cancellationToken);
        return mapper.Map<UserGroupDto>(groupWithConnectedEntities);
    }
    
    public async Task<List<UserGroupDto>> GetAll(UserGroupFilterDto? filter, CancellationToken cancellationToken)
    {
        var predicate = UserGroupFilterHelper.CreatePredicate(filter);

        var entities = await base.GetAll(predicate, cancellationToken);
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

    public async Task<UserGroupDto> ShareSubscription(Guid groupId, Guid subscriptionId, CancellationToken cancellationToken)
    {
        var group = await userGroupRepository.GetById(groupId, cancellationToken)
                ?? throw new NotFoundException($"Group with id {groupId} not found.");
        
        if (group.SharedSubscriptions is not null && group.SharedSubscriptions.Any(s => s.Id == subscriptionId))
        {
            throw new InvalidOperationException($"Subscription with id {subscriptionId} is already shared with group {groupId}");
        }
        
        var subscription = await subscriptionRepository.GetById(subscriptionId, cancellationToken)
                           ?? throw new NotFoundException($"Subscription with id {subscriptionId} not found.");
        
        group.SharedSubscriptions.Add(subscription);

        var updatedGroup = await userGroupRepository.Update(group, cancellationToken);
        return mapper.Map<UserGroupDto>(updatedGroup);
    }

    public async Task<UserGroupDto> UnshareSubscription(Guid groupId, Guid subscriptionId, CancellationToken cancellationToken)
    {
        var group = await userGroupRepository.GetById(groupId, cancellationToken)
                    ?? throw new NotFoundException($"Group with id {groupId} not found.");

        var subscriptionToRemove = group.SharedSubscriptions.FirstOrDefault(s => s.Id == subscriptionId);
        
        if (group.SharedSubscriptions is null)
        {
            throw new InvalidOperationException($"No subscriptions is shared in group with id {groupId}");
        }

        if (subscriptionToRemove is null)
        {
            throw new InvalidOperationException($"No subscription is shared in group with id {groupId}");
        }
        
        group.SharedSubscriptions.Remove(subscriptionToRemove);

        var updatedGroup = await userGroupRepository.Update(group, cancellationToken);
        return mapper.Map<UserGroupDto>(updatedGroup);
    }
}
