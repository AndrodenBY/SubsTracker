using AutoMapper;
using DispatchR;
using SubsTracker.BLL.DTOs.User;
using SubsTracker.BLL.DTOs.User.Create;
using SubsTracker.BLL.DTOs.User.Update;
using SubsTracker.BLL.Handlers.Signals.Group;
using SubsTracker.BLL.Helpers.Filters;
using SubsTracker.BLL.Interfaces.Cache;
using SubsTracker.BLL.Interfaces.User;
using SubsTracker.BLL.RedisSettings;
using SubsTracker.DAL.Interfaces;
using SubsTracker.DAL.Interfaces.Repositories;
using SubsTracker.DAL.Models.User;
using SubsTracker.Domain.Enums;
using SubsTracker.Domain.Exceptions;
using SubsTracker.Domain.Filter;
using SubsTracker.Domain.Pagination;

namespace SubsTracker.BLL.Services.User;

public class UserGroupService(
    IUserGroupRepository groupRepository,
    IUserRepository userRepository,
    ISubscriptionRepository subscriptionRepository,
    IGroupMemberService memberService,
    IMapper mapper,
    ICacheService cacheService,
    IMediator mediator) 
    : Service<UserGroup, UserGroupDto, CreateUserGroupDto, UpdateUserGroupDto, UserGroupFilterDto>(groupRepository, mapper, cacheService),
    IUserGroupService
{
    public async Task<UserGroupDto?> GetFullInfoById(Guid id, CancellationToken cancellationToken)
    {
        var cacheKey = RedisKeySetter.SetCacheKey<UserGroup>(id);

        async Task<UserGroupDto?> GetUserGroup()
        {
            var groupWithEntities = await groupRepository.GetFullInfoById(id, cancellationToken);
            return Mapper.Map<UserGroupDto>(groupWithEntities);
        }
        
        return await CacheService.CacheDataWithLock(cacheKey, GetUserGroup, cancellationToken);
    }

    public async Task<PaginatedList<UserGroupDto>> GetAll(UserGroupFilterDto? filter, PaginationParameters? paginationParameters, CancellationToken cancellationToken)
    {
        var predicate = UserGroupFilterHelper.CreatePredicate(filter);
        return await base.GetAll(predicate, paginationParameters, cancellationToken);
    }

    public async Task<UserGroupDto> Create(string auth0Id, CreateUserGroupDto createDto, CancellationToken cancellationToken)
    {
        var existingUser = await userRepository.GetByAuth0Id(auth0Id, cancellationToken)
                           ?? throw new InvalidRequestDataException($"User with id {auth0Id} does not exist");
        createDto.UserId = existingUser.Id;

        var createdGroup = await base.Create(createDto, cancellationToken);

        var createMemberDto = new CreateGroupMemberDto
        {
            UserId = existingUser.Id,
            GroupId = createdGroup.Id,
            Role = MemberRole.Admin
        };
        await memberService.Create(createMemberDto, cancellationToken);
        
        await mediator.Publish(new GroupCreatedSignal(createdGroup.Id, existingUser.Id), cancellationToken);
        return createdGroup;
    }

    public new async Task<UserGroupDto> Update(Guid updateId, UpdateUserGroupDto updateDto, CancellationToken cancellationToken)
    {
        var existingUserGroup = await groupRepository.GetById(updateId, cancellationToken)
                                ?? throw new UnknownIdentifierException($"UserGroup with id {updateId} not found");

        Mapper.Map(updateDto, existingUserGroup);
        var updatedEntity = await groupRepository.Update(existingUserGroup, cancellationToken);

        await mediator.Publish(new GroupUpdatedSignal(updateId, existingUserGroup.UserId 
                                                                ?? throw new UnknownIdentifierException($"User with {existingUserGroup.UserId} not found")), cancellationToken);
        return Mapper.Map<UserGroupDto>(updatedEntity);
    }

    public async Task<UserGroupDto> ShareSubscription(Guid groupId, Guid subscriptionId, CancellationToken cancellationToken)
    {
        var group = await groupRepository.GetFullInfoById(groupId, cancellationToken)
                    ?? throw new UnknownIdentifierException($"Group with id {groupId} not found.");

        if (group.SharedSubscriptions is not null && group.SharedSubscriptions.Any(s => s.Id == subscriptionId))
            throw new PolicyViolationException(
                $"Subscription with id {subscriptionId} is already shared with group {groupId}");

        var subscription = await subscriptionRepository.GetById(subscriptionId, cancellationToken)
                           ?? throw new UnknownIdentifierException($"Subscription with id {subscriptionId} not found.");

        group.SharedSubscriptions?.Add(subscription);
        var updatedGroup = await groupRepository.Update(group, cancellationToken);
        
        await mediator.Publish(new GroupUpdatedSignal(groupId, group.UserId
                                                               ?? throw new UnknownIdentifierException($"User with {group.UserId} not found")), cancellationToken);
        return Mapper.Map<UserGroupDto>(updatedGroup);
    }

    public async Task<UserGroupDto> UnshareSubscription(Guid groupId, Guid subscriptionId, CancellationToken cancellationToken)
    {
        var group = await groupRepository.GetFullInfoById(groupId, cancellationToken)
                    ?? throw new UnknownIdentifierException($"Group with id {groupId} not found.");

        var subscriptionToRemove = group.SharedSubscriptions?.FirstOrDefault(s => s.Id == subscriptionId);

        if (subscriptionToRemove is null)
            throw new ArgumentException($"No subscription is shared in group with id {groupId}");

        group.SharedSubscriptions?.Remove(subscriptionToRemove);
        var updatedGroup = await groupRepository.Update(group, cancellationToken);
        
        await mediator.Publish(new GroupUpdatedSignal(groupId, group.UserId
                                                               ?? throw new UnknownIdentifierException($"User with {group.UserId} not found")), cancellationToken);
        return Mapper.Map<UserGroupDto>(updatedGroup);
    }

    public new async Task<bool> Delete(Guid id, CancellationToken cancellationToken)
    {
        var existingUserGroup = await groupRepository.GetById(id, cancellationToken)
                                ?? throw new UnknownIdentifierException($"UserGroup with id {id} not found");

        await mediator.Publish(new GroupDeletedSignal(id, existingUserGroup.UserId 
                                                          ?? throw new UnknownIdentifierException($"User with {existingUserGroup.UserId} not found")), cancellationToken);
        return await groupRepository.Delete(existingUserGroup, cancellationToken);
    }
}
