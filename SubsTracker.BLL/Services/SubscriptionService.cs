using AutoMapper;
using SubsTracker.BLL.DTOs.Subscription;
using SubsTracker.BLL.Helpers;
using SubsTracker.BLL.Helpers.Filters;
using SubsTracker.BLL.Helpers.Notifications;
using SubsTracker.BLL.Interfaces;
using SubsTracker.BLL.Interfaces.Cache;
using SubsTracker.BLL.RedisSettings;
using SubsTracker.DAL.Entities;
using SubsTracker.DAL.Interfaces.Repositories;
using SubsTracker.Domain.Enums;
using SubsTracker.Domain.Exceptions;
using SubsTracker.Domain.Filter;
using SubsTracker.Domain.Pagination;
using SubsTracker.Messaging.Interfaces;

namespace SubsTracker.BLL.Services;

public class SubscriptionService(
    ISubscriptionRepository subscriptionRepository,
    IMessageService messageService,
    IMapper mapper,
    IUserRepository userRepository,
    ISubscriptionHistoryRepository historyRepository,
    ICacheService cacheService,
    ICacheAccessService cacheAccessService) 
    : Service<SubscriptionEntity, SubscriptionDto, CreateSubscriptionDto, UpdateSubscriptionDto, SubscriptionFilterDto>(subscriptionRepository, mapper, cacheService),
      ISubscriptionService
{
    public async Task<SubscriptionDto?> GetUserInfoById(Guid id, CancellationToken cancellationToken)
    {
        var cacheKey = RedisKeySetter.SetCacheKey<SubscriptionDto>(id);
        return await CacheService.CacheDataWithLock(cacheKey, GetSubscription, cancellationToken);
        
        async Task<SubscriptionDto?> GetSubscription()
        {
            var subscriptionWithEntities = await subscriptionRepository.GetUserInfoById(id, cancellationToken);
            return Mapper.Map<SubscriptionDto>(subscriptionWithEntities);
        }
    }

    public async Task<PaginatedList<SubscriptionDto>> GetAll(SubscriptionFilterDto? filter, PaginationParameters? paginationParameters, CancellationToken cancellationToken)  
    {
        var expression = SubscriptionFilterHelper.CreatePredicate(filter);
        return await base.GetAll(expression, paginationParameters, cancellationToken);
    }

    public async Task<SubscriptionDto> Create(string auth0Id, CreateSubscriptionDto createDto, CancellationToken cancellationToken)
    {
        var existingUser = await userRepository.GetByAuth0Id(auth0Id, cancellationToken)
                           ?? throw new UnknownIdentifierException($"User with id {auth0Id} does not exist");
        
        await SubscriptionPolicyChecker.PreventSubscriptionDuplication(subscriptionRepository, existingUser.Id, createDto.Name, cancellationToken);
        
        var subscriptionToCreate = Mapper.Map<SubscriptionEntity>(createDto);
        subscriptionToCreate.UserId = existingUser.Id;

        var createdSubscription = await subscriptionRepository.Create(subscriptionToCreate, cancellationToken);

        await historyRepository.Create(createdSubscription.Id, SubscriptionAction.Activate, createDto.Price, cancellationToken);
        return Mapper.Map<SubscriptionDto>(createdSubscription);
    }

    public async Task<SubscriptionDto> Update(string auth0Id, UpdateSubscriptionDto updateDto, CancellationToken cancellationToken)
    {
        var (originalSubscription, _) = 
            await SubscriptionPolicyChecker.GetValidatedSubscription(userRepository, subscriptionRepository, auth0Id, updateDto.Id, cancellationToken);

        Mapper.Map(updateDto, originalSubscription);
        var updatedSubscription = await subscriptionRepository.Update(originalSubscription, cancellationToken);

        await historyRepository.UpdateType(originalSubscription.Type, updatedSubscription.Type, updatedSubscription.Id, updatedSubscription.Price, cancellationToken);
        return Mapper.Map<SubscriptionDto>(updatedSubscription);
    }

    public async Task<SubscriptionDto> CancelSubscription(string auth0Id, Guid subscriptionId, CancellationToken cancellationToken)
    {
        var (subscription, user) = 
            await SubscriptionPolicyChecker.GetValidatedSubscription(userRepository, subscriptionRepository, auth0Id, subscriptionId, cancellationToken);
        
        subscription.Active = false;
        var canceledSubscription = await subscriptionRepository.Update(subscription, cancellationToken);

        await historyRepository.Create(canceledSubscription.Id, SubscriptionAction.Cancel, null, cancellationToken);
        await HandlePostCancellationActions(canceledSubscription, user.Id, cancellationToken);
        return Mapper.Map<SubscriptionDto>(canceledSubscription);
    }

    public async Task<SubscriptionDto> RenewSubscription(Guid subscriptionId, int monthsToRenew, CancellationToken cancellationToken)
    {
        if (monthsToRenew <= 0) throw new InvalidRequestDataException("Cannot renew subscription for less than one month");

        var subscriptionToRenew = await subscriptionRepository.GetUserInfoById(subscriptionId, cancellationToken)
                                  ?? throw new UnknownIdentifierException($"Subscription with id {subscriptionId} not found");

        subscriptionToRenew.DueDate = subscriptionToRenew.DueDate.AddMonths(monthsToRenew);
        subscriptionToRenew.Active = true;
        var renewedSubscription = await subscriptionRepository.Update(subscriptionToRenew, cancellationToken);
        await historyRepository.Create(renewedSubscription.Id, SubscriptionAction.Renew, renewedSubscription.Price, cancellationToken);

        var subscriptionRenewedEvent = SubscriptionNotificationHelper.CreateSubscriptionRenewedEvent(renewedSubscription);
        await messageService.NotifySubscriptionRenewed(subscriptionRenewedEvent, cancellationToken);
        return Mapper.Map<SubscriptionDto>(renewedSubscription);
    }

    public async Task<List<SubscriptionDto>> GetUpcomingBills(string auth0Id, CancellationToken cancellationToken)
    {
        var existingUser = await userRepository.GetByAuth0Id(auth0Id, cancellationToken)
            ?? throw new UnknownIdentifierException($"User with  {auth0Id} not found");
        var cacheKey = RedisKeySetter.SetCacheKey(existingUser.Id, "upcoming_bills");
        var cachedList = await cacheAccessService.GetData<List<SubscriptionDto>>(cacheKey, cancellationToken);
        if (cachedList is not null) return cachedList;

        var billsToPay = await subscriptionRepository.GetUpcomingBills(existingUser.Id, cancellationToken)
                         ?? throw new UnknownIdentifierException($"Subscriptions with UserId {existingUser.Id} not found");

        var mappedList = Mapper.Map<List<SubscriptionDto>>(billsToPay);
        await cacheAccessService.SetData(cacheKey, mappedList, RedisConstants.ExpirationTime, cancellationToken);
        return mappedList;
    }

    private async Task HandlePostCancellationActions(SubscriptionEntity canceledSubscriptionEntity, Guid userId, CancellationToken cancellationToken)
    {
        var subscriptionCanceledEvent = SubscriptionNotificationHelper.CreateSubscriptionCanceledEvent(canceledSubscriptionEntity);
        await messageService.NotifySubscriptionCanceled(subscriptionCanceledEvent, cancellationToken);

        var keysToRemove = new List<string>
        {
            RedisKeySetter.SetCacheKey<SubscriptionDto>(canceledSubscriptionEntity.Id),
            RedisKeySetter.SetCacheKey(userId, "upcoming_bills")
        };

        await cacheAccessService.RemoveData(keysToRemove, cancellationToken);
    }
}
