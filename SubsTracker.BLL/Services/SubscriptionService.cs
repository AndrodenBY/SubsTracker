using AutoMapper;
using DispatchR;
using SubsTracker.BLL.DTOs.Subscription;
using SubsTracker.BLL.Filter;
using SubsTracker.BLL.Helpers.Filters;
using SubsTracker.BLL.Helpers.Policy;
using SubsTracker.BLL.Interfaces;
using SubsTracker.BLL.Interfaces.Cache;
using SubsTracker.BLL.Mediator.Handlers.UpcomingBills;
using SubsTracker.BLL.Mediator.Signals;
using SubsTracker.BLL.RedisSettings;
using SubsTracker.DAL.Entities;
using SubsTracker.DAL.Interfaces.Repositories;
using SubsTracker.Domain.Enums;
using SubsTracker.Domain.Exceptions;
using SubsTracker.Domain.Pagination;

namespace SubsTracker.BLL.Services;

public class SubscriptionService(
    ISubscriptionRepository subscriptionRepository,
    IUserRepository userRepository,
    IMapper mapper,
    ICacheService cacheService, 
    IMediator mediator) 
    : Service<SubscriptionEntity, SubscriptionDto, CreateSubscriptionDto, UpdateSubscriptionDto, SubscriptionFilterDto>(subscriptionRepository, mapper, cacheService),
      ISubscriptionService
{
    public async Task<SubscriptionDto?> GetUserInfoById(Guid id, CancellationToken cancellationToken)
    {
        var cacheKey = RedisKeySetter.SetCacheKey<SubscriptionEntity>(id);
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

    public async Task<SubscriptionDto> Create(Guid userId, CreateSubscriptionDto createDto, CancellationToken cancellationToken)
    {
        var existingUser = await userRepository.GetById(userId, cancellationToken)
                           ?? throw new UnknownIdentifierException($"User with id {userId} does not exist");
        
        await SubscriptionPolicyChecker.PreventSubscriptionDuplication(subscriptionRepository, existingUser.Id, createDto.Name, cancellationToken);
        
        var subscriptionToCreate = Mapper.Map<SubscriptionEntity>(createDto);
        subscriptionToCreate.UserId = existingUser.Id;
        subscriptionToCreate.DueDate = subscriptionToCreate.Type is SubscriptionType.Lifetime
            ? subscriptionToCreate.DueDate = DateOnly.MaxValue
            : createDto.DueDate;

        var createdSubscription = await subscriptionRepository.Create(subscriptionToCreate, cancellationToken);

        await mediator.Publish(new SubscriptionSignals.Created(createdSubscription, existingUser.Id), cancellationToken);
        return Mapper.Map<SubscriptionDto>(createdSubscription);
    }

    public new async Task<SubscriptionDto> Update(Guid userId, UpdateSubscriptionDto updateDto, CancellationToken cancellationToken)
    {
        var originalSubscription = await SubscriptionPolicyChecker.GetValidatedSubscription(userRepository, subscriptionRepository, userId, updateDto.Id, cancellationToken);

        Mapper.Map(updateDto, originalSubscription);
        var updated = await subscriptionRepository.Update(originalSubscription, cancellationToken);
        
        await mediator.Publish(new SubscriptionSignals.Updated(updated, originalSubscription.Type, userId), cancellationToken);
        return Mapper.Map<SubscriptionDto>(updated);
    }

    public async Task<SubscriptionDto> CancelSubscription(Guid userId, Guid subscriptionId, CancellationToken cancellationToken)
    {
        var subscription = await SubscriptionPolicyChecker.GetValidatedSubscription(userRepository, subscriptionRepository, userId, subscriptionId, cancellationToken);
        
        subscription.Active = false;
        subscription.DueDate = DateOnly.FromDateTime(DateTime.UtcNow);
        
        var canceledSubscription = await subscriptionRepository.Update(subscription, cancellationToken);

        await mediator.Publish(new SubscriptionSignals.Canceled(canceledSubscription, userId), cancellationToken);
        return Mapper.Map<SubscriptionDto>(canceledSubscription);
    }

    public async Task<SubscriptionDto> RenewSubscription(Guid subscriptionId, int monthsToRenew, CancellationToken cancellationToken)
    {
        var subscriptionToRenew = await subscriptionRepository.GetUserInfoById(subscriptionId, cancellationToken)
                                  ?? throw new UnknownIdentifierException($"Subscription with id {subscriptionId} not found");
        
        if (monthsToRenew < 1)
        {
            throw new InvalidRequestDataException("Cannot renew subscription for less than one month");
        }

        if (subscriptionToRenew.Type is SubscriptionType.Trial or SubscriptionType.Lifetime)
        {
            throw new PolicyViolationException("This type of subscription cannot be renewed");
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var baseDate = subscriptionToRenew.DueDate > today 
            ? subscriptionToRenew.DueDate 
            : today;
        
        subscriptionToRenew.DueDate = subscriptionToRenew.Type is SubscriptionType.Lifetime
            ? DateOnly.MaxValue
            : baseDate.AddMonths(monthsToRenew);
        
        subscriptionToRenew.Active = true;
        
        var renewedSubscription = await subscriptionRepository.Update(subscriptionToRenew, cancellationToken);
        
        await mediator.Publish(new SubscriptionSignals.Renewed(renewedSubscription, renewedSubscription.UserId
                ?? throw new InvalidOperationException("UserId cannot be null")), cancellationToken);
        return Mapper.Map<SubscriptionDto>(renewedSubscription);
    }

    public async Task<List<SubscriptionDto>> GetUpcomingBills(Guid userId, CancellationToken cancellationToken)
    {
        return await mediator.Send(new GetUpcomingBills(userId), cancellationToken);
    }
}
