using AutoMapper;
using SubsTracker.BLL.DTOs.Subscription;
using SubsTracker.BLL.Helpers.Filters;
using SubsTracker.BLL.Interfaces.Subscription;
using SubsTracker.DAL.Interfaces.Repositories;
using SubsTracker.Domain.Enums;
using SubsTracker.Domain.Exceptions;
using SubsTracker.Domain.Filter;
//using SubsTracker.Messaging.Interfaces;
using SubscriptionModel = SubsTracker.DAL.Models.Subscription.Subscription;
using UserModel = SubsTracker.DAL.Models.User.User;

namespace SubsTracker.BLL.Services.Subscription;

public class SubscriptionService(
    ISubscriptionRepository subscriptionRepository,
    IRepository<SubscriptionModel> genericRepository,
    //IMessageService messageService,
    IMapper mapper,
    IRepository<UserModel> userRepository,
    ISubscriptionHistoryRepository historyRepository
    ) : Service<SubscriptionModel, SubscriptionDto, CreateSubscriptionDto, UpdateSubscriptionDto, SubscriptionFilterDto>(subscriptionRepository, mapper),
    ISubscriptionService
{
    private new ISubscriptionRepository SubscriptionRepository => (ISubscriptionRepository)base.Repository;

    public override async Task<SubscriptionDto?> GetById(Guid id, CancellationToken cancellationToken)
    {
        var subscriptionWithConnectedEntities = await SubscriptionRepository.GetById(id, cancellationToken);
        return Mapper.Map<SubscriptionDto>(subscriptionWithConnectedEntities);
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

        var createdSubscription = await SubscriptionRepository.Create(subscriptionToCreate, cancellationToken);
        var subscriptionDto = Mapper.Map<SubscriptionDto>(createdSubscription);

        await historyRepository.Create(createdSubscription.Id, SubscriptionAction.Activate, createDto.Price, cancellationToken);
        return subscriptionDto;
    }

    public override async Task<SubscriptionDto> Update(Guid userId, UpdateSubscriptionDto updateDto, CancellationToken cancellationToken)
    {
        var userWithSubscription = await userRepository.GetById(userId, cancellationToken)
            ?? throw new NotFoundException($"User with id {userId} does not exist");

        var originalSubscription = await genericRepository.GetById(updateDto.Id, cancellationToken)
            ?? throw new NotFoundException($"Subscription with id {updateDto.Id} not found");

        if (originalSubscription.UserId != userWithSubscription.Id)
        {
            throw new NotFoundException($"Subscription with id {updateDto.Id} does not belong to user {userWithSubscription.Id}");
        }

        Mapper.Map(updateDto, originalSubscription);
        var updatedSubscription = await SubscriptionRepository.Update(originalSubscription, cancellationToken);
        
        await historyRepository.UpdateType(originalSubscription.Type, updatedSubscription.Type, updatedSubscription.Id, updatedSubscription.Price, cancellationToken);
        return Mapper.Map<SubscriptionDto>(updatedSubscription);
    }

    public async Task<SubscriptionDto> CancelSubscription(Guid userId, Guid subscriptionId, CancellationToken cancellationToken)
    {
        var subscription = await SubscriptionRepository.GetById(subscriptionId, cancellationToken)
                           ?? throw new NotFoundException($"Subscription with id {subscriptionId} not found");

        if (subscription.UserId != userId)
        {
            throw new NotFoundException($"Subscription with id {subscriptionId} does not belong to user {userId}");
        }

        subscription.Active = false;
        
        var updatedSubscription = await SubscriptionRepository.Update(subscription, cancellationToken);

        await historyRepository.Create(updatedSubscription.Id, SubscriptionAction.Cancel, null, cancellationToken);
        //await messageService.NotifySubscriptionCanceled(subscription, cancellationToken);
        return Mapper.Map<SubscriptionDto>(updatedSubscription);
    }

    public async Task<SubscriptionDto> RenewSubscription(Guid subscriptionId, int monthsToRenew, CancellationToken cancellationToken)
    {
        if (monthsToRenew <= 0)
        {
            throw new ValidationException("Cannot renew subscription for less than one month");
        }

        var subscriptionToRenew = await SubscriptionRepository.GetById(subscriptionId, cancellationToken)
                                  ?? throw new NotFoundException($"Subscription with id {subscriptionId} not found");

        subscriptionToRenew.DueDate = subscriptionToRenew.DueDate.AddMonths(monthsToRenew);
        subscriptionToRenew.Active = true;
        var renewedSubscription = await SubscriptionRepository.Update(subscriptionToRenew, cancellationToken);
        await historyRepository.Create(renewedSubscription.Id, SubscriptionAction.Renew, renewedSubscription.Price, cancellationToken);

        //await messageService.NotifySubscriptionRenewed(renewedSubscription, cancellationToken);
        var subscriptionDto = Mapper.Map<SubscriptionDto>(renewedSubscription);
        return subscriptionDto;
    }

    public async Task<List<SubscriptionDto>> GetUpcomingBills(Guid userId, CancellationToken cancellationToken)
    {
        var billsToPay = await SubscriptionRepository.GetUpcomingBills(userId, cancellationToken)
            ?? throw new NotFoundException($"Subscriptions with UserId {userId} not found");

        return Mapper.Map<List<SubscriptionDto>>(billsToPay);
    }
}
