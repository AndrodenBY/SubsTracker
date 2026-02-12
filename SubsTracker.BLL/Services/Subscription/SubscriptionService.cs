using AutoMapper;
using SubsTracker.BLL.DTOs.Subscription;
using SubsTracker.BLL.Helpers.Filters;
using SubsTracker.BLL.Helpers.Notifications;
using SubsTracker.BLL.Interfaces.Cache;
using SubsTracker.BLL.Interfaces.Subscription;
using SubsTracker.BLL.RedisSettings;
using SubsTracker.DAL.Interfaces;
using SubsTracker.DAL.Interfaces.Repositories;
using SubsTracker.Domain.Enums;
using SubsTracker.Domain.Exceptions;
using SubsTracker.Domain.Filter;
using SubsTracker.Messaging.Interfaces;
using UserModel = SubsTracker.DAL.Models.User.User;
using SubscriptionModel = SubsTracker.DAL.Models.Subscription.Subscription;

namespace SubsTracker.BLL.Services.Subscription;

public class SubscriptionService(
    ISubscriptionRepository subscriptionRepository,
    IMessageService messageService,
    IMapper mapper,
    IUserRepository userRepository,
    ISubscriptionHistoryRepository historyRepository,
    ICacheService cacheService,
    ICacheAccessService cacheAccessService
) : Service<SubscriptionModel, SubscriptionDto, CreateSubscriptionDto, UpdateSubscriptionDto, SubscriptionFilterDto>(subscriptionRepository, mapper, cacheService),
    ISubscriptionService
{
    public async Task<SubscriptionDto?> GetUserInfoById(Guid id, CancellationToken cancellationToken)
    {
        var cacheKey = RedisKeySetter.SetCacheKey<SubscriptionDto>(id);
        async Task<SubscriptionDto?> GetSubscription()
        {
            var subscriptionWithEntities = await subscriptionRepository.GetUserInfoById(id, cancellationToken);
            return Mapper.Map<SubscriptionDto>(subscriptionWithEntities);
        }
        
        return await CacheService.CacheDataWithLock(cacheKey, RedisConstants.ExpirationTime, GetSubscription, cancellationToken);
    }

    public async Task<List<SubscriptionDto>> GetAll(SubscriptionFilterDto? filter, CancellationToken cancellationToken)
    {
        var predicate = SubscriptionFilterHelper.CreatePredicate(filter);
        return await base.GetAll(predicate, cancellationToken);
    }

    public async Task<SubscriptionDto> Create(string auth0Id, CreateSubscriptionDto createDto, CancellationToken cancellationToken)
    {
        var existingUser = await userRepository.GetByAuth0Id(auth0Id, cancellationToken)
                           ?? throw new UnknowIdentifierException($"User with id {auth0Id} does not exist");
        await PreventSubscriptionDuplication(existingUser.Id, createDto.Name, cancellationToken);
        
        var subscriptionToCreate = Mapper.Map<SubscriptionModel>(createDto);
        subscriptionToCreate.UserId = existingUser.Id;

        var createdSubscription = await subscriptionRepository.Create(subscriptionToCreate, cancellationToken);

        await historyRepository.Create(createdSubscription.Id, SubscriptionAction.Activate, createDto.Price, cancellationToken);
        return Mapper.Map<SubscriptionDto>(createdSubscription);
    }

    public async Task<SubscriptionDto> Update(string auth0Id, UpdateSubscriptionDto updateDto, CancellationToken cancellationToken)
    {
        var (originalSubscription, _) = await GetValidatedSubscription(auth0Id, updateDto.Id, cancellationToken);

        Mapper.Map(updateDto, originalSubscription);
        var updatedSubscription = await subscriptionRepository.Update(originalSubscription, cancellationToken);

        await historyRepository.UpdateType(originalSubscription.Type, updatedSubscription.Type, updatedSubscription.Id, updatedSubscription.Price, cancellationToken);
        return Mapper.Map<SubscriptionDto>(updatedSubscription);
    }

    public async Task<SubscriptionDto> CancelSubscription(string auth0Id, Guid subscriptionId, CancellationToken cancellationToken)
    {
        var (subscription, user) = await GetValidatedSubscription(auth0Id, subscriptionId, cancellationToken);
        
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
                                  ?? throw new UnknowIdentifierException($"Subscription with id {subscriptionId} not found");

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
            ?? throw new UnknowIdentifierException($"User with  {auth0Id} not found");
        var cacheKey = RedisKeySetter.SetCacheKey(existingUser.Id, "upcoming_bills");
        var cachedList = await cacheAccessService.GetData<List<SubscriptionDto>>(cacheKey, cancellationToken);
        if (cachedList is not null) return cachedList;

        var billsToPay = await subscriptionRepository.GetUpcomingBills(existingUser.Id, cancellationToken)
                         ?? throw new UnknowIdentifierException($"Subscriptions with UserId {existingUser.Id} not found");

        var mappedList = Mapper.Map<List<SubscriptionDto>>(billsToPay);
        await cacheAccessService.SetData(cacheKey, mappedList, RedisConstants.ExpirationTime, cancellationToken);
        return mappedList;
    }

    private async Task HandlePostCancellationActions(SubscriptionModel canceledSubscription, Guid userId, CancellationToken cancellationToken)
    {
        var subscriptionCanceledEvent = SubscriptionNotificationHelper.CreateSubscriptionCanceledEvent(canceledSubscription);
        await messageService.NotifySubscriptionCanceled(subscriptionCanceledEvent, cancellationToken);

        var keysToRemove = new List<string>
        {
            RedisKeySetter.SetCacheKey<SubscriptionDto>(canceledSubscription.Id),
            RedisKeySetter.SetCacheKey(userId, "upcoming_bills")
        };

        await cacheAccessService.RemoveData(keysToRemove, cancellationToken);
    }
    
    private async Task<(SubscriptionModel Subscription, UserModel User)> GetValidatedSubscription(string auth0Id, Guid subscriptionId, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByAuth0Id(auth0Id, cancellationToken)
                   ?? throw new UnknowIdentifierException($"User {auth0Id} not found");

        var subscription = await subscriptionRepository.GetById(subscriptionId, cancellationToken)
                           ?? throw new UnknowIdentifierException($"Subscription {subscriptionId} not found");
        
        if (!subscription.UserId.HasValue || subscription.UserId.Value != user.Id)
        {
            throw new ForbiddenException($"User {user.Id} does not own subscription {subscriptionId}");
        }
        
        return (subscription, user);
    }
    
    private async Task PreventSubscriptionDuplication(Guid userId, string subscriptionName, CancellationToken cancellationToken)
    {
        var existingSub = await subscriptionRepository.GetByPredicate(
            subscription => subscription.UserId == userId && subscription.Name == subscriptionName && subscription.Active, 
            cancellationToken);
        
        if (existingSub is not null)
        {
            throw new PolicyViolationException($"A subscription named '{subscriptionName}' already exists for this user.");
        }
    }
}
