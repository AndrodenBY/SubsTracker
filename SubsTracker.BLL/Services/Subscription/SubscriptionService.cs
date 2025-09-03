using System.Linq.Expressions;
using AutoMapper;
using LinqKit;
using SubsTracker.BLL.DTOs.Subscription;
using SubsTracker.BLL.Interfaces;
using SubsTracker.DAL.Interfaces;
using SubsTracker.DAL.Repository;
using SubsTracker.Domain.Enums;
using SubsTracker.Domain.Exceptions;
using SubsTracker.Domain.Filter;
using SubscriptionModel = SubsTracker.DAL.Models.Subscription.Subscription;

namespace SubsTracker.BLL.Services.Subscription;

public class SubscriptionService(ISubscriptionRepository repository, IMapper mapper, ISubscriptionHistoryRepository history)
    : Service<SubscriptionModel, SubscriptionDto, CreateSubscriptionDto, UpdateSubscriptionDto, SubscriptionFilter>(repository, mapper), ISubscriptionService
{
    public async Task<IEnumerable<SubscriptionDto>> GetAll(SubscriptionFilter? filter, CancellationToken cancellationToken)
    {
        var predicate = CreatePredicate(filter);
        
        var entities = await base.GetAll(predicate, cancellationToken);
        return entities;
    }

    private static Expression<Func<SubscriptionModel, bool>> CreatePredicate(SubscriptionFilter filter)
    {
        var predicate = PredicateBuilder.New<SubscriptionModel>(true);
        
        predicate = AddFilterCondition<SubscriptionModel>(
            predicate, 
            filter.Name, 
            subscription => subscription.Name.Contains(filter.Name!, StringComparison.OrdinalIgnoreCase)
        );
        
        predicate = AddFilterCondition<SubscriptionModel, decimal>(
            predicate, 
            filter.Price, 
            subscription => subscription.Price == filter.Price!.Value
        );
        
        predicate = AddFilterCondition<SubscriptionModel, SubscriptionType>(
            predicate, 
            filter.Type, 
            subscription => subscription.Type == filter.Type!.Value
        );
        
        predicate = AddFilterCondition<SubscriptionModel, SubscriptionContent>(
            predicate, 
            filter.Content, 
            subscription => subscription.Content == filter.Content!.Value
        );

        return predicate;
    }
    
    public async Task<SubscriptionDto> Create(Guid userId, CreateSubscriptionDto createDto, CancellationToken cancellationToken)
    {
        var entityToCreate = mapper.Map<SubscriptionModel>(createDto);
        entityToCreate.UserId = userId;

        var createdEntity = await repository.Create(entityToCreate, cancellationToken);
        var subscriptionDto = mapper.Map<SubscriptionDto>(createdEntity);

        await history.Create(createdEntity.Id, SubscriptionAction.Activation, createDto.Price, cancellationToken);
        return subscriptionDto;
    }

    public override async Task<SubscriptionDto> Update(Guid userId, UpdateSubscriptionDto updateDto, CancellationToken cancellationToken)
    {
        var originalSubscription = await repository.GetById(updateDto.Id, cancellationToken);
        if (originalSubscription == null)
        {
            throw new NotFoundException($"Subscription with id {updateDto.Id} not found");
        }
        
        var updatedSubscription = await base.Update(updateDto.Id, updateDto, cancellationToken);
        await history.UpdateType(originalSubscription.Type, updatedSubscription.Type, updatedSubscription.Id, updatedSubscription.Price, cancellationToken);
        return updatedSubscription;
    }

    public override async Task<bool> Delete(Guid id, CancellationToken cancellationToken)
    {
        var subscriptionDeleted = await base.Delete(id, cancellationToken);
        
        await history.Create(id, SubscriptionAction.Cancellation, null, cancellationToken);
        return subscriptionDeleted;
    }

    public async Task<SubscriptionDto> RenewSubscription(Guid subscriptionId, int monthsToRenew,
        CancellationToken cancellationToken)
    {
        if (monthsToRenew <= 0) throw new ValidationException("Cannot renew subscription for less than one month");
        
        var subscriptionToRenew = await repository.GetById(subscriptionId, cancellationToken)
                                  ?? throw new NotFoundException($"Subscription with id {subscriptionId} not found");
        
        subscriptionToRenew.DueDate = subscriptionToRenew.DueDate.AddMonths(monthsToRenew);
        var renewedSubscription = await repository.Update(subscriptionToRenew, cancellationToken);
        await history.Create(renewedSubscription.Id, SubscriptionAction.Renewal, renewedSubscription.Price, cancellationToken);
        
        var subscriptionDto = mapper.Map<SubscriptionDto>(renewedSubscription);
        return subscriptionDto;
    }

    public async Task<IEnumerable<SubscriptionDto>> GetUpcomingBills(Guid userId,
        CancellationToken cancellationToken)
    {
        var billsToPay = await repository.GetUpcomingBills(userId, cancellationToken)
            ?? throw new NotFoundException($"User with id {userId} not found");
        return mapper.Map<IEnumerable<SubscriptionDto>>(billsToPay);
    }
}