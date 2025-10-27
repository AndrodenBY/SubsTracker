using AutoMapper;
using SubsTracker.BLL.DTOs.User;
using SubsTracker.BLL.DTOs.User.Create;
using SubsTracker.BLL.DTOs.User.Update;
using SubsTracker.BLL.Helpers.Filters;
using SubsTracker.BLL.Interfaces.Cache;
using SubsTracker.BLL.Interfaces.User;
using SubsTracker.BLL.RedisSettings;
using SubsTracker.DAL.Interfaces.Repositories;
using SubsTracker.DAL.Models.User;
using SubsTracker.Domain.Enums;
using SubsTracker.Domain.Exceptions;
using SubsTracker.Domain.Filter;
using UserModel = SubsTracker.DAL.Models.User.User;
using InvalidOperationException = SubsTracker.Domain.Exceptions.InvalidOperationException;

namespace SubsTracker.BLL.Services.User;

public class UserGroupService(
    IUserGroupRepository groupRepository,
    IRepository<UserModel> userRepository,
    ISubscriptionRepository subscriptionRepository,
    IGroupMemberService memberService,
    IMapper mapper,
    ICacheService cacheService
    ) : Service<UserGroup, UserGroupDto, CreateUserGroupDto, UpdateUserGroupDto, UserGroupFilterDto>(groupRepository, mapper, cacheService),
    IUserGroupService
{
    public async Task<UserGroupDto?> GetFullInfoById(Guid id, CancellationToken cancellationToken)
    {
        var cacheKey = RedisKeySetter.SetCacheKey<UserGroupDto>(id);
        return await CacheService.CacheDataWithLock(cacheKey, RedisConstants.ExpirationTime, GetUserGroup, cancellationToken);

        async Task<UserGroupDto> GetUserGroup()
        {
            var groupWithEntities = await groupRepository.GetFullInfoById(id, cancellationToken);
            return Mapper.Map<UserGroupDto>(groupWithEntities);
        }
    }

    public async Task<List<UserGroupDto>> GetAll(UserGroupFilterDto? filter, CancellationToken cancellationToken)
    {
        var predicate = UserGroupFilterHelper.CreatePredicate(filter);
        return await base.GetAll(predicate, cancellationToken);
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

    public new async Task<UserGroupDto> Update(Guid updateId, UpdateUserGroupDto updateDto, CancellationToken cancellationToken)
    {
        var existingUserGroup = await groupRepository.GetById(updateId, cancellationToken)
                             ?? throw new NotFoundException($"UserGroup with id {updateId} not found");

        Mapper.Map(updateDto, existingUserGroup);
        var updatedEntity = await groupRepository.Update(existingUserGroup, cancellationToken);

        return Mapper.Map<UserGroupDto>(updatedEntity);
    }

    public async Task<UserGroupDto> ShareSubscription(Guid groupId, Guid subscriptionId, CancellationToken cancellationToken)
    {
        var group = await groupRepository.GetFullInfoById(groupId, cancellationToken)
                ?? throw new NotFoundException($"Group with id {groupId} not found.");

        if (group.SharedSubscriptions is not null && group.SharedSubscriptions.Any(s => s.Id == subscriptionId))
        {
            throw new InvalidOperationException($"Subscription with id {subscriptionId} is already shared with group {groupId}");
        }

        var subscription = await subscriptionRepository.GetById(subscriptionId, cancellationToken)
                           ?? throw new NotFoundException($"Subscription with id {subscriptionId} not found.");

        group.SharedSubscriptions?.Add(subscription);

        var updatedGroup = await groupRepository.Update(group, cancellationToken);
        return Mapper.Map<UserGroupDto>(updatedGroup);
    }

    public async Task<UserGroupDto> UnshareSubscription(Guid groupId, Guid subscriptionId, CancellationToken cancellationToken)
    {
        var group = await groupRepository.GetFullInfoById(groupId, cancellationToken)
                    ?? throw new NotFoundException($"Group with id {groupId} not found.");

        var subscriptionToRemove = group.SharedSubscriptions?.FirstOrDefault(s => s.Id == subscriptionId);

        if (subscriptionToRemove is null)
        {
            throw new ArgumentException($"No subscription is shared in group with id {groupId}");
        }

        group.SharedSubscriptions?.Remove(subscriptionToRemove);

        var updatedGroup = await groupRepository.Update(group, cancellationToken);
        return Mapper.Map<UserGroupDto>(updatedGroup);
    }

    public new async Task<bool> Delete(Guid id, CancellationToken cancellationToken)
    {
        var existingUserGroup = await groupRepository.GetById(id, cancellationToken)
                             ?? throw new NotFoundException($"UserGroup with id {id} not found");

        return await groupRepository.Delete(existingUserGroup, cancellationToken);
    }
}
