using AutoMapper;
using SubsTracker.BLL.DTOs.Subscription;
using SubsTracker.BLL.Helpers.Filters;
using SubsTracker.BLL.Interfaces.Subscription;
using SubsTracker.DAL.Interfaces.Repositories;
using SubsTracker.Domain.Enums;
using SubsTracker.Domain.Exceptions;
using SubsTracker.Domain.Filter;
using SubscriptionModel = SubsTracker.DAL.Models.Subscription.Subscription;

namespace SubsTracker.BLL.Services.Subscription;

public class SubscriptionService(
    ISubscriptionRepository repository,
    IMapper mapper,
    IRepository<DAL.Models.User.User> userRepository,
    ISubscriptionHistoryRepository history
    ) : Service<SubscriptionModel, SubscriptionDto, CreateSubscriptionDto, UpdateSubscriptionDto, SubscriptionFilterDto>(repository, mapper),
    ISubscriptionService
{
    public async Task<List<SubscriptionDto>> GetAll(SubscriptionFilterDto? filter, CancellationToken cancellationToken)
    {
        var predicate = SubscriptionFilterHelper.CreatePredicate(filter);

        var entities = await base.GetAll(predicate, cancellationToken);
        return entities;
    }

    public async Task<SubscriptionDto> Create(Guid userId, CreateSubscriptionDto createDto, CancellationToken cancellationToken)
    {
        var existingUser = await base.GetById(userId, cancellationToken)
            ?? throw new NotFoundException($"User with id {userId} does not exist");
        
        var subscriptionToCreate = mapper.Map<SubscriptionModel>(createDto);
        subscriptionToCreate.UserId = existingUser.Id;
    
        var createdSubscription = await repository.Create(subscriptionToCreate, cancellationToken);
        var subscriptionDto = mapper.Map<SubscriptionDto>(createdSubscription);

        await history.Create(createdSubscription.Id, SubscriptionAction.Activate, createDto.Price, cancellationToken);
        return subscriptionDto;
    }

    public override async Task<SubscriptionDto> Update(Guid userId, UpdateSubscriptionDto updateDto, CancellationToken cancellationToken)
    {
        var userWithSubscription = await userRepository.GetById(userId, cancellationToken)
            ?? throw new NotFoundException($"User with id {userId} does not exist");
        
        var originalSubscription = await repository
                                       .GetByPredicate(s => s.Id == updateDto.Id && s.UserId == userWithSubscription.Id, cancellationToken) 
                                   ?? throw new NotFoundException($"Subscription with id {updateDto.Id} not found or does not belong to user {userWithSubscription.Id}");

        var updatedSubscription = await base.Update(updateDto.Id, updateDto, cancellationToken);
        await history.UpdateType(originalSubscription.Type, updatedSubscription.Type, updatedSubscription.Id, updatedSubscription.Price, cancellationToken);
        return updatedSubscription;
    }

    public async Task<SubscriptionDto> CancelSubscription(Guid userId, Guid subscriptionId, CancellationToken cancellationToken)
    {
        var subscription = await repository
            .GetByPredicate(subscription => subscription.Id == subscriptionId && subscription.UserId == userId, cancellationToken);

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

    public async Task<List<SubscriptionDto>> GetUpcomingBills(Guid userId, CancellationToken cancellationToken)
    {
        var billsToPay = await repository.GetUpcomingBills(userId, cancellationToken)
            ?? throw new NotFoundException($"User with id {userId} not found");
        return mapper.Map<List<SubscriptionDto>>(billsToPay);
    }
}
