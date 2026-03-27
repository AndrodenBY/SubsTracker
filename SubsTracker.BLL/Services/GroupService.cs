using AutoMapper;
using DispatchR;
using SubsTracker.BLL.DTOs.User;
using SubsTracker.BLL.DTOs.User.Create;
using SubsTracker.BLL.DTOs.User.Update;
using SubsTracker.BLL.Filter;
using SubsTracker.BLL.Helpers.Filters;
using SubsTracker.BLL.Interfaces;
using SubsTracker.BLL.Interfaces.Cache;
using SubsTracker.BLL.Mediator.Signals;
using SubsTracker.BLL.RedisSettings;
using SubsTracker.DAL.Entities;
using SubsTracker.DAL.Interfaces.Repositories;
using SubsTracker.Domain.Enums;
using SubsTracker.Domain.Exceptions;
using SubsTracker.Domain.Pagination;

namespace SubsTracker.BLL.Services;

public class GroupService(
    IGroupRepository groupRepository,
    IUserRepository userRepository,
    ISubscriptionRepository subscriptionRepository,
    IMemberService memberService,
    IMapper mapper,
    ICacheService cacheService,
    IMediator mediator) 
    : IGroupService
{
    public async Task<GroupDto?> GetFullInfoById(Guid id, CancellationToken cancellationToken)
    {
        var cacheKey = RedisKeySetter.SetCacheKey<GroupEntity>(id);
        return await cacheService.CacheDataWithLock(cacheKey, GetUserGroup, cancellationToken);
        
        async Task<GroupDto?> GetUserGroup()
        {
            var groupWithEntities = await groupRepository.GetFullInfoById(id, cancellationToken);
            return mapper.Map<GroupDto>(groupWithEntities);
        }
    }
    
    public async Task<GroupDto> GetById(Guid id, CancellationToken cancellationToken)
    {
        var cacheKey = RedisKeySetter.SetCacheKey<GroupEntity>(id);
        var groupDto = await cacheService.CacheDataWithLock(cacheKey, GetEntity, cancellationToken)
                     ?? throw new UnknownIdentifierException($"Group with {id} not found");
        
        return groupDto;
        
        async Task<GroupDto?> GetEntity()
        {
            var group = await groupRepository.GetById(id, cancellationToken);
            return mapper.Map<GroupDto>(group);
        }
    }

    public async Task<PaginatedList<GroupDto>> GetAll(GroupFilter? filter, PaginationParameters? paginationParameters, CancellationToken cancellationToken)  
    {
        var expression = GroupFilterHelper.CreatePredicate(filter);
        var pagedGroups = await groupRepository.GetAll(expression, paginationParameters, cancellationToken);
        return pagedGroups.MapToPage(mapper.Map<GroupDto>);
    }

    public async Task<GroupDto> Create(Guid userId, CreateGroupDto createDto, CancellationToken cancellationToken)
    {
        var existingUser = await userRepository.GetById(userId, cancellationToken)
                           ?? throw new InvalidRequestDataException($"User with id {userId} does not exist");
        
        createDto.UserId = existingUser.Id;
        var mappedGroup = mapper.Map<GroupEntity>(createDto);
        
        var createdGroup = await groupRepository.Create(mappedGroup, cancellationToken);

        var createMemberDto = new CreateMemberDto
        {
            UserId = existingUser.Id,
            GroupId = createdGroup.Id,
            Role = MemberRole.Admin
        };
        await memberService.Create(createMemberDto, cancellationToken);

        await mediator.Publish(new GroupSignals.Created(createdGroup.Id, existingUser.Id), cancellationToken);
        return mapper.Map<GroupDto>(createdGroup);
    }

    public async Task<GroupDto> Update(Guid updateId, UpdateGroupDto updateDto, CancellationToken cancellationToken)
    {
        var existingUserGroup = await groupRepository.GetById(updateId, cancellationToken)
                                ?? throw new UnknownIdentifierException($"UserGroup with id {updateId} not found");

        mapper.Map(updateDto, existingUserGroup);
        var updatedEntity = await groupRepository.Update(existingUserGroup, cancellationToken);

        await mediator.Publish(new GroupSignals.Updated(updatedEntity.Id, updatedEntity.UserId 
                       ?? throw new UnknownIdentifierException($"Group with UserId {updatedEntity.UserId} does not exist")), cancellationToken);
        return mapper.Map<GroupDto>(updatedEntity);
    }

    public async Task<GroupDto> ShareSubscription(Guid groupId, Guid subscriptionId, CancellationToken cancellationToken)
    {
        var group = await groupRepository.GetFullInfoById(groupId, cancellationToken)
                    ?? throw new UnknownIdentifierException($"Group with id {groupId} not found.");

        if (group.SharedSubscriptions is not null && group.SharedSubscriptions.Any(s => s.Id == subscriptionId))
        {
            throw new PolicyViolationException($"Subscription with id {subscriptionId} is already shared with group {groupId}");
        }

        var subscription = await subscriptionRepository.GetById(subscriptionId, cancellationToken)
                           ?? throw new UnknownIdentifierException($"Subscription with id {subscriptionId} not found.");

        group.SharedSubscriptions?.Add(subscription);

        var updatedGroup = await groupRepository.Update(group, cancellationToken);
        
        await mediator.Publish(new GroupSignals.Updated(groupId, group.UserId
                       ?? throw new UnknownIdentifierException($"User with {group.UserId} not found")), cancellationToken);
        return mapper.Map<GroupDto>(updatedGroup);
    }

    public async Task<GroupDto> UnshareSubscription(Guid groupId, Guid subscriptionId, CancellationToken cancellationToken)
    {
        var group = await groupRepository.GetFullInfoById(groupId, cancellationToken)
                    ?? throw new UnknownIdentifierException($"Group with id {groupId} not found.");

        var subscriptionToRemove = group.SharedSubscriptions?.FirstOrDefault(s => s.Id == subscriptionId) 
                                   ?? throw new ArgumentException($"No subscription is shared in group with id {groupId}");

        group.SharedSubscriptions?.Remove(subscriptionToRemove);

        var updatedGroup = await groupRepository.Update(group, cancellationToken);
        
        await mediator.Publish(new GroupSignals.Updated(groupId, group.UserId
                       ?? throw new UnknownIdentifierException($"User with {group.UserId} not found")), cancellationToken);
        return mapper.Map<GroupDto>(updatedGroup);
    }

    public async Task<bool> Delete(Guid id, CancellationToken cancellationToken)
    {
        var existingUserGroup = await groupRepository.GetById(id, cancellationToken)
                                ?? throw new UnknownIdentifierException($"UserGroup with id {id} not found");

        await mediator.Publish(new GroupSignals.Deleted(id, existingUserGroup.UserId 
                       ?? throw new UnknownIdentifierException($"User with {existingUserGroup.UserId} not found")), cancellationToken);
        return await groupRepository.Delete(existingUserGroup, cancellationToken);
    }
}
