using AutoMapper;
using DispatchR;
using SubsTracker.BLL.DispatchR.Handlers.UpcomingBills;
using SubsTracker.BLL.DispatchR.Signals;
using SubsTracker.BLL.DTOs.Subscription;
using SubsTracker.BLL.Helpers;
using SubsTracker.BLL.Helpers.Filters;
using SubsTracker.BLL.Interfaces;
using SubsTracker.BLL.Interfaces.Cache;
using SubsTracker.BLL.RedisSettings;
using SubsTracker.DAL.Entities;
using SubsTracker.DAL.Interfaces.Repositories;
using SubsTracker.Domain.Exceptions;
using SubsTracker.Domain.Filter;
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

    public async Task<SubscriptionDto> Create(string auth0Id, CreateSubscriptionDto createDto, CancellationToken cancellationToken)
    {
        var existingUser = await userRepository.GetByAuth0Id(auth0Id, cancellationToken)
                           ?? throw new UnknownIdentifierException($"User with id {auth0Id} does not exist");
        
        await SubscriptionPolicyChecker.PreventSubscriptionDuplication(subscriptionRepository, existingUser.Id, createDto.Name, cancellationToken);
        
        var subscriptionToCreate = Mapper.Map<SubscriptionEntity>(createDto);
        subscriptionToCreate.UserId = existingUser.Id;

        var createdSubscription = await subscriptionRepository.Create(subscriptionToCreate, cancellationToken);

        await mediator.Publish(new SubscriptionSignals.Created(createdSubscription, existingUser.Id), cancellationToken);
        return Mapper.Map<SubscriptionDto>(createdSubscription);
    }

    public async Task<SubscriptionDto> Update(string auth0Id, UpdateSubscriptionDto updateDto, CancellationToken cancellationToken)
    {
        var (originalSubscription, user) = await SubscriptionPolicyChecker.GetValidatedSubscription(userRepository, subscriptionRepository, auth0Id, updateDto.Id, cancellationToken);

        Mapper.Map(updateDto, originalSubscription);
        var updated = await subscriptionRepository.Update(originalSubscription, cancellationToken);
        
        await mediator.Publish(new SubscriptionSignals.Updated(updated, originalSubscription.Type, user.Id), cancellationToken);
        return Mapper.Map<SubscriptionDto>(updated);
    }

    public async Task<SubscriptionDto> CancelSubscription(string auth0Id, Guid subscriptionId, CancellationToken cancellationToken)
    {
        var (subscription, user) = await SubscriptionPolicyChecker.GetValidatedSubscription(userRepository, subscriptionRepository, auth0Id, subscriptionId, cancellationToken);
        
        subscription.Active = false;
        var canceledSubscription = await subscriptionRepository.Update(subscription, cancellationToken);

        await mediator.Publish(new SubscriptionSignals.Canceled(canceledSubscription, user.Id), cancellationToken);
        return Mapper.Map<SubscriptionDto>(canceledSubscription);
    }

    public async Task<SubscriptionDto> RenewSubscription(Guid subscriptionId, int monthsToRenew, CancellationToken cancellationToken)
    {
        if (monthsToRenew <= 1)
        {
            throw new InvalidRequestDataException("Cannot renew subscription for less than one month");
        }

        var subscriptionToRenew = await subscriptionRepository.GetUserInfoById(subscriptionId, cancellationToken)
                                  ?? throw new UnknownIdentifierException($"Subscription with id {subscriptionId} not found");

        subscriptionToRenew.DueDate = subscriptionToRenew.DueDate.AddMonths(monthsToRenew);
        subscriptionToRenew.Active = true;
        var renewedSubscription = await subscriptionRepository.Update(subscriptionToRenew, cancellationToken);
        
        await mediator.Publish(new SubscriptionSignals.Renewed(renewedSubscription, renewedSubscription.UserId
                ?? throw new InvalidOperationException("UserId cannot be null")), cancellationToken);
        return Mapper.Map<SubscriptionDto>(renewedSubscription);
    }

    public async Task<List<SubscriptionDto>> GetUpcomingBills(string auth0Id, CancellationToken cancellationToken)
    {
        return await mediator.Send(new GetUpcomingBills(auth0Id), cancellationToken);
    }
}
