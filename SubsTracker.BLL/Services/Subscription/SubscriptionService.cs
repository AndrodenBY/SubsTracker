using AutoMapper;
using SubsTracker.BLL.DTOs.Subscription;
using SubsTracker.BLL.Helpers.Filters;
using SubsTracker.BLL.Helpers.Notifications;
using SubsTracker.BLL.Interfaces.Cache;
using SubsTracker.BLL.Interfaces.Subscription;
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
    ICacheService cacheService
    ) : Service<SubscriptionModel, SubscriptionDto, CreateSubscriptionDto, UpdateSubscriptionDto, SubscriptionFilterDto>(subscriptionRepository, mapper, cacheService),
    ISubscriptionService
{
    public async Task<SubscriptionDto?> GetUserInfoById(Guid id, CancellationToken cancellationToken)
    {
        var cacheKey = $"{id}_{nameof(SubscriptionDto)}";
        var cachedDto = await CacheService.GetData<SubscriptionDto>(cacheKey, cancellationToken);
        
        if (cachedDto is not null)
        {
            return cachedDto;
        }
        
        var subscriptionWithConnectedEntities = await subscriptionRepository.GetUserInfoById(id, cancellationToken);
        var mappedSubscription = Mapper.Map<SubscriptionDto>(subscriptionWithConnectedEntities);
        
        await CacheService.SetData(cacheKey, mappedSubscription, TimeSpan.FromMinutes(3), cancellationToken);
        return mappedSubscription;
    }
    
    public async Task<List<SubscriptionDto>> GetAll(SubscriptionFilterDto? filter, CancellationToken cancellationToken)
    {
        var predicate = SubscriptionFilterHelper.CreatePredicate(filter);

        var entities = await base.GetAll(predicate, cancellationToken);
        return entities;
    }

    public async Task<SubscriptionDto> Create(Guid userId, CreateSubscriptionDto createDto, CancellationToken cancellationToken)
    {
        var existingUser = await userRepository.GetById(userId, cancellationToken)
            ?? throw new NotFoundException($"User with id {userId} does not exist");

        var subscriptionToCreate = Mapper.Map<SubscriptionModel>(createDto);
        subscriptionToCreate.UserId = existingUser.Id;

        var createdSubscription = await subscriptionRepository.Create(subscriptionToCreate, cancellationToken);
        var subscriptionDto = Mapper.Map<SubscriptionDto>(createdSubscription);

        await historyRepository.Create(createdSubscription.Id, SubscriptionAction.Activate, createDto.Price, cancellationToken);
        return subscriptionDto;
    }

    public override async Task<SubscriptionDto> Update(Guid userId, UpdateSubscriptionDto updateDto, CancellationToken cancellationToken)
    {
        var userWithSubscription = await userRepository.GetById(userId, cancellationToken)
            ?? throw new NotFoundException($"User with id {userId} does not exist");

        var originalSubscription = await subscriptionRepository.GetById(updateDto.Id, cancellationToken)
            ?? throw new NotFoundException($"Subscription with id {updateDto.Id} not found");

        if (originalSubscription.UserId != userWithSubscription.Id)
        {
            throw new NotFoundException($"Subscription with id {updateDto.Id} does not belong to user {userWithSubscription.Id}");
        }

        Mapper.Map(updateDto, originalSubscription);
        var updatedSubscription = await subscriptionRepository.Update(originalSubscription, cancellationToken);
        
        await historyRepository.UpdateType(originalSubscription.Type, updatedSubscription.Type, updatedSubscription.Id, updatedSubscription.Price, cancellationToken);
        return Mapper.Map<SubscriptionDto>(updatedSubscription);
    }

    public async Task<SubscriptionDto> CancelSubscription(Guid userId, Guid subscriptionId, CancellationToken cancellationToken)
    {
        var subscription = await subscriptionRepository.GetUserInfoById(subscriptionId, cancellationToken)
                           ?? throw new NotFoundException($"Subscription with id {subscriptionId} not found");

        if (subscription.UserId != userId)
        {
            throw new NotFoundException($"Subscription with id {subscriptionId} does not belong to user {userId}");
        }

        subscription.Active = false;
        
        var canceledSubscription = await subscriptionRepository.Update(subscription, cancellationToken);

        await historyRepository.Create(canceledSubscription.Id, SubscriptionAction.Cancel, null, cancellationToken);
        var subscriptionCanceledEvent = SubscriptionNotificationHelper.CreateSubscriptionCanceledEvent(canceledSubscription);
        await messageService.NotifySubscriptionCanceled(subscriptionCanceledEvent, cancellationToken);
        
        await CacheService.RemoveData($"{subscriptionId}_{nameof(SubscriptionDto)}", cancellationToken);
        await CacheService.RemoveData($"{userId}_upcoming_bills", cancellationToken);
        
        return Mapper.Map<SubscriptionDto>(canceledSubscription);
    }

    public async Task<SubscriptionDto> RenewSubscription(Guid subscriptionId, int monthsToRenew, CancellationToken cancellationToken)
    {
        if (monthsToRenew <= 0)
        {
            throw new ValidationException("Cannot renew subscription for less than one month");
        }

        var subscriptionToRenew = await subscriptionRepository.GetUserInfoById(subscriptionId, cancellationToken)
                                  ?? throw new NotFoundException($"Subscription with id {subscriptionId} not found");

        subscriptionToRenew.DueDate = subscriptionToRenew.DueDate.AddMonths(monthsToRenew);
        subscriptionToRenew.Active = true;
        var renewedSubscription = await subscriptionRepository.Update(subscriptionToRenew, cancellationToken);
        await historyRepository.Create(renewedSubscription.Id, SubscriptionAction.Renew, renewedSubscription.Price, cancellationToken);

        var subscriptionRenewedEvent = SubscriptionNotificationHelper.CreateSubscriptionRenewedEvent(renewedSubscription);
        await messageService.NotifySubscriptionRenewed(subscriptionRenewedEvent, cancellationToken);
        var subscriptionDto = Mapper.Map<SubscriptionDto>(renewedSubscription);
        return subscriptionDto;
    }

    public async Task<List<SubscriptionDto>> GetUpcomingBills(Guid userId, CancellationToken cancellationToken)
    {
        var cacheKey = $"{userId}_upcoming_bills";
        var cachedList = await CacheService.GetData<List<SubscriptionDto>>(cacheKey, cancellationToken);
        if (cachedList is not null)
        {
            return cachedList;
        }
        
        var billsToPay = await subscriptionRepository.GetUpcomingBills(userId, cancellationToken)
            ?? throw new NotFoundException($"Subscriptions with UserId {userId} not found");

        var mappedList = Mapper.Map<List<SubscriptionDto>>(billsToPay);
        await CacheService.SetData(cacheKey, mappedList, TimeSpan.FromMinutes(3), cancellationToken);
        return mappedList;
    }
}
