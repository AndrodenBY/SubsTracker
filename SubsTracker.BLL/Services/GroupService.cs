using AutoMapper;
using SubsTracker.BLL.DTOs.User;
using SubsTracker.BLL.DTOs.User.Create;
using SubsTracker.BLL.DTOs.User.Update;
using SubsTracker.BLL.Helpers.Filters;
using SubsTracker.BLL.Interfaces;
using SubsTracker.BLL.Interfaces.Cache;
using SubsTracker.BLL.RedisSettings;
using SubsTracker.DAL.Entities;
using SubsTracker.DAL.Interfaces.Repositories;
using SubsTracker.Domain.Enums;
using SubsTracker.Domain.Exceptions;
using SubsTracker.Domain.Filter;

namespace SubsTracker.BLL.Services;

public class GroupService(
    IGroupRepository groupRepository,
    IUserRepository userRepository,
    ISubscriptionRepository subscriptionRepository,
    IMemberService memberService,
    IMapper mapper,
    ICacheService cacheService
) : Service<GroupEntity, GroupDto, CreateGroupDto, UpdateGroupDto, GroupFilterDto>(groupRepository, mapper, cacheService),
    IGroupService
{
    public async Task<GroupDto?> GetFullInfoById(Guid id, CancellationToken cancellationToken)
    {
        var cacheKey = RedisKeySetter.SetCacheKey<GroupDto>(id);
        return await CacheService.CacheDataWithLock(cacheKey, RedisConstants.ExpirationTime, GetUserGroup, cancellationToken);
        
        async Task<GroupDto?> GetUserGroup()
        {
            var groupWithEntities = await groupRepository.GetFullInfoById(id, cancellationToken);
            return Mapper.Map<GroupDto>(groupWithEntities);
        }
    }

    public async Task<List<GroupDto>> GetAll(GroupFilterDto? filter, CancellationToken cancellationToken)
    {
        var expression = GroupFilterHelper.CreatePredicate(filter);
        return await base.GetAll(expression, cancellationToken);
    }

    public async Task<GroupDto> Create(string auth0Id, CreateGroupDto createDto, CancellationToken cancellationToken)
    {
        var existingUser = await userRepository.GetByAuth0Id(auth0Id, cancellationToken)
                           ?? throw new InvalidRequestDataException($"User with id {auth0Id} does not exist");
        createDto.UserId = existingUser.Id;

        var createdGroup = await base.Create(createDto, cancellationToken);

        var createMemberDto = new CreateMemberDto
        {
            UserId = existingUser.Id,
            GroupId = createdGroup.Id,
            Role = MemberRole.Admin
        };
        await memberService.Create(createMemberDto, cancellationToken);

        return createdGroup;
    }

    public new async Task<GroupDto> Update(Guid updateId, UpdateGroupDto updateDto, CancellationToken cancellationToken)
    {
        var existingUserGroup = await groupRepository.GetById(updateId, cancellationToken)
                                ?? throw new UnknownIdentifierException($"UserGroup with id {updateId} not found");

        Mapper.Map(updateDto, existingUserGroup);
        var updatedEntity = await groupRepository.Update(existingUserGroup, cancellationToken);

        return Mapper.Map<GroupDto>(updatedEntity);
    }

    public async Task<GroupDto> ShareSubscription(Guid groupId, Guid subscriptionId, CancellationToken cancellationToken)
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
        return Mapper.Map<GroupDto>(updatedGroup);
    }

    public async Task<GroupDto> UnshareSubscription(Guid groupId, Guid subscriptionId, CancellationToken cancellationToken)
    {
        var group = await groupRepository.GetFullInfoById(groupId, cancellationToken)
                    ?? throw new UnknownIdentifierException($"Group with id {groupId} not found.");

        var subscriptionToRemove = group.SharedSubscriptions?.FirstOrDefault(s => s.Id == subscriptionId);

        if (subscriptionToRemove is null)
            throw new ArgumentException($"No subscription is shared in group with id {groupId}");

        group.SharedSubscriptions?.Remove(subscriptionToRemove);

        var updatedGroup = await groupRepository.Update(group, cancellationToken);
        return Mapper.Map<GroupDto>(updatedGroup);
    }

    public new async Task<bool> Delete(Guid id, CancellationToken cancellationToken)
    {
        var existingUserGroup = await groupRepository.GetById(id, cancellationToken)
                                ?? throw new UnknownIdentifierException($"UserGroup with id {id} not found");

        return await groupRepository.Delete(existingUserGroup, cancellationToken);
    }
}
