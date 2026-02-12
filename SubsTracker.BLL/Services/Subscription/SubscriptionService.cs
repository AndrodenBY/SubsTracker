using AutoMapper;
using SubsTracker.BLL.DTOs.Subscription;
using SubsTracker.BLL.Helpers.Filters;
using SubsTracker.BLL.Helpers.Notifications;
using SubsTracker.BLL.Interfaces.Cache;
using SubsTracker.BLL.Interfaces.Subscription;
using SubsTracker.BLL.RedisSettings;
using SubsTracker.DAL.Interfaces.Repositories;
using SubsTracker.Domain.Enums;
using SubsTracker.Domain.Exceptions;
using SubsTracker.Domain.Filter;
using SubsTracker.Messaging.Interfaces;
using SubscriptionModel = SubsTracker.DAL.Models.Subscription.Subscription;
using UserModel = SubsTracker.DAL.Models.User.User;

namespace SubsTracker.BLL.Services.Subscription;

public class SubscriptionService(
    ISubscriptionRepository subscriptionRepository,
    IMessageService messageService,
    IMapper mapper,
    IRepository<UserModel> userRepository,
    ISubscriptionHistoryRepository historyRepository,
    ICacheService cacheService,
    ICacheAccessService cacheAccessService
) : Service<SubscriptionModel, SubscriptionDto, CreateSubscriptionDto, UpdateSubscriptionDto, SubscriptionFilterDto>(
        subscriptionRepository, mapper, cacheService),
    ISubscriptionService
{
    public async Task<SubscriptionDto?> GetUserInfoById(Guid id, CancellationToken cancellationToken)
    {
        var cacheKey = RedisKeySetter.SetCacheKey<SubscriptionDto>(id);
        return await CacheService.CacheDataWithLock(cacheKey, RedisConstants.ExpirationTime, GetSubscription,
            cancellationToken);

        async Task<SubscriptionDto?> GetSubscription()
        {
            var subscriptionWithEntities = await subscriptionRepository.GetUserInfoById(id, cancellationToken);
            return Mapper.Map<SubscriptionDto>(subscriptionWithEntities);
        }
    }

    public async Task<List<SubscriptionDto>> GetAll(SubscriptionFilterDto? filter, CancellationToken cancellationToken)
    {
        var predicate = SubscriptionFilterHelper.CreatePredicate(filter);
        return await base.GetAll(predicate, cancellationToken);
    }

    public async Task<SubscriptionDto> Create(Guid userId, CreateSubscriptionDto createDto,
        CancellationToken cancellationToken)
    {
        var existingUser = await userRepository.GetById(userId, cancellationToken)
                           ?? throw new NotFoundException($"User with id {userId} does not exist");

        var subscriptionToCreate = Mapper.Map<SubscriptionModel>(createDto);
        subscriptionToCreate.UserId = existingUser.Id;

        var createdSubscription = await subscriptionRepository.Create(subscriptionToCreate, cancellationToken);

        await historyRepository.Create(createdSubscription.Id, SubscriptionAction.Activate, createDto.Price,
            cancellationToken);
        return Mapper.Map<SubscriptionDto>(createdSubscription);
    }

    public override async Task<SubscriptionDto> Update(Guid userId, UpdateSubscriptionDto updateDto,
        CancellationToken cancellationToken)
    {
        var userWithSubscription = await userRepository.GetById(userId, cancellationToken)
                                   ?? throw new NotFoundException($"User with id {userId} does not exist");

        var originalSubscription = await subscriptionRepository.GetById(updateDto.Id, cancellationToken)
                                   ?? throw new NotFoundException($"Subscription with id {updateDto.Id} not found");

        if (originalSubscription.UserId != userWithSubscription.Id)
            throw new NotFoundException(
                $"Subscription with id {updateDto.Id} does not belong to user {userWithSubscription.Id}");

        Mapper.Map(updateDto, originalSubscription);
        var updatedSubscription = await subscriptionRepository.Update(originalSubscription, cancellationToken);

        await historyRepository.UpdateType(originalSubscription.Type, updatedSubscription.Type, updatedSubscription.Id,
            updatedSubscription.Price, cancellationToken);
        return Mapper.Map<SubscriptionDto>(updatedSubscription);
    }

    public async Task<SubscriptionDto> CancelSubscription(Guid userId, Guid subscriptionId,
        CancellationToken cancellationToken)
    {
        var subscription = await subscriptionRepository.GetUserInfoById(subscriptionId, cancellationToken)
                           ?? throw new NotFoundException($"Subscription with id {subscriptionId} not found");

        if (subscription.UserId != userId)
            throw new NotFoundException($"Subscription with id {subscriptionId} does not belong to user {userId}");

        subscription.Active = false;
        var canceledSubscription = await subscriptionRepository.Update(subscription, cancellationToken);

        await historyRepository.Create(canceledSubscription.Id, SubscriptionAction.Cancel, null, cancellationToken);
        await HandlePostCancellationActions(canceledSubscription, userId, cancellationToken);
        return Mapper.Map<SubscriptionDto>(canceledSubscription);
    }

    public async Task<SubscriptionDto> RenewSubscription(Guid subscriptionId, int monthsToRenew,
        CancellationToken cancellationToken)
    {
        if (monthsToRenew <= 0) throw new ValidationException("Cannot renew subscription for less than one month");

        var subscriptionToRenew = await subscriptionRepository.GetUserInfoById(subscriptionId, cancellationToken)
                                  ?? throw new NotFoundException($"Subscription with id {subscriptionId} not found");

        subscriptionToRenew.DueDate = subscriptionToRenew.DueDate.AddMonths(monthsToRenew);
        subscriptionToRenew.Active = true;
        var renewedSubscription = await subscriptionRepository.Update(subscriptionToRenew, cancellationToken);
        await historyRepository.Create(renewedSubscription.Id, SubscriptionAction.Renew, renewedSubscription.Price,
            cancellationToken);

        var subscriptionRenewedEvent =
            SubscriptionNotificationHelper.CreateSubscriptionRenewedEvent(renewedSubscription);
        await messageService.NotifySubscriptionRenewed(subscriptionRenewedEvent, cancellationToken);
        return Mapper.Map<SubscriptionDto>(renewedSubscription);
    }

    public async Task<List<SubscriptionDto>> GetUpcomingBills(Guid userId, CancellationToken cancellationToken)
    {
        var cacheKey = RedisKeySetter.SetCacheKey(userId, "upcoming_bills");
        var cachedList = await cacheAccessService.GetData<List<SubscriptionDto>>(cacheKey, cancellationToken);
        if (cachedList is not null) return cachedList;

        var billsToPay = await subscriptionRepository.GetUpcomingBills(userId, cancellationToken)
                         ?? throw new NotFoundException($"Subscriptions with UserId {userId} not found");

        var mappedList = Mapper.Map<List<SubscriptionDto>>(billsToPay);
        await cacheAccessService.SetData(cacheKey, mappedList, RedisConstants.ExpirationTime, cancellationToken);
        return mappedList;
    }

    private async Task HandlePostCancellationActions(SubscriptionModel canceledSubscription, Guid userId,
        CancellationToken cancellationToken)
    {
        var subscriptionCanceledEvent =
            SubscriptionNotificationHelper.CreateSubscriptionCanceledEvent(canceledSubscription);
        await messageService.NotifySubscriptionCanceled(subscriptionCanceledEvent, cancellationToken);

        var keysToRemove = new List<string>
        {
            RedisKeySetter.SetCacheKey<SubscriptionDto>(canceledSubscription.Id),
            RedisKeySetter.SetCacheKey(userId, "upcoming_bills")
        };

        await cacheAccessService.RemoveData(keysToRemove, cancellationToken);
    }
}
