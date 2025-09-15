using AutoMapper;
using SubsTracker.BLL.DTOs.Subscription;
using SubsTracker.BLL.Interfaces;
using SubsTracker.BLL.Interfaces.Subscription;
using SubsTracker.DAL.Interfaces.Repositories;
using SubsTracker.Domain.Enums;
using SubsTracker.Domain.Exceptions;
using SubscriptionModel = SubsTracker.DAL.Models.Subscription.Subscription;

namespace SubsTracker.BLL.Services.Subscription;

public class SubscriptionService(
    ISubscriptionRepository repository, 
    IMapper mapper, 
    ISubscriptionHistoryRepository history
    ) : Service<SubscriptionModel, SubscriptionDto, CreateSubscriptionDto, UpdateSubscriptionDto, SubscriptionFilterDto>(repository, mapper), 
    ISubscriptionService
{
    public async Task<SubscriptionDto> Create(Guid userId, CreateSubscriptionDto createDto, CancellationToken cancellationToken)
    {
        var entityToCreate = mapper.Map<SubscriptionModel>(createDto);
        entityToCreate.UserId = userId;

        var createdEntity = await repository.Create(entityToCreate, cancellationToken);
        var subscriptionDto = mapper.Map<SubscriptionDto>(createdEntity);

        await history.Create(createdEntity.Id, SubscriptionAction.Activate, createDto.Price, cancellationToken);
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
    
    public async Task<SubscriptionDto> CancelSubscription(Guid id, CancellationToken cancellationToken)
    {
        var subscription = await repository.GetById(id, cancellationToken)
                           ?? throw new NotFoundException($"Subscription with id {id} not found");
        
        subscription.Active = false;
        var updatedSubscription = await repository.Update(subscription, cancellationToken);
        
        await history.Create(updatedSubscription.Id, SubscriptionAction.Cancel, null, cancellationToken);
        return mapper.Map<SubscriptionDto>(updatedSubscription);
    }

    public async Task<SubscriptionDto> RenewSubscription(Guid subscriptionId, int monthsToRenew,
        CancellationToken cancellationToken)
    {
        if (monthsToRenew <= 0)
        {
            throw new ValidationException("Cannot renew subscription for less than one month");
        }
        
        var subscriptionToRenew = await repository.GetById(subscriptionId, cancellationToken)
                                  ?? throw new NotFoundException($"Subscription with id {subscriptionId} not found");
        
        subscriptionToRenew.DueDate = subscriptionToRenew.DueDate.AddMonths(monthsToRenew);
        var renewedSubscription = await repository.Update(subscriptionToRenew, cancellationToken);
        await history.Create(renewedSubscription.Id, SubscriptionAction.Renew, renewedSubscription.Price, cancellationToken);
        
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
