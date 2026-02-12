using SubsTracker.BLL.DTOs.Subscription;
using SubsTracker.Domain.Filter;
using SubscriptionModel = SubsTracker.DAL.Models.Subscription.Subscription;

namespace SubsTracker.BLL.Interfaces.Subscription;

public interface ISubscriptionService :
    IService<SubscriptionModel, SubscriptionDto, CreateSubscriptionDto, UpdateSubscriptionDto, SubscriptionFilterDto>
{
    Task<SubscriptionDto?> GetUserInfoById(Guid id, CancellationToken cancellationToken);
    Task<List<SubscriptionDto>> GetAll(SubscriptionFilterDto? filter, CancellationToken cancellationToken);
    Task<SubscriptionDto> Create(string auth0Id, CreateSubscriptionDto createDto, CancellationToken cancellationToken);
    Task<SubscriptionDto> Update(string auth0Id, UpdateSubscriptionDto updateDto, CancellationToken cancellationToken);
    Task<SubscriptionDto> CancelSubscription(string auth0Id, Guid subscriptionId, CancellationToken cancellationToken);
    Task<SubscriptionDto> RenewSubscription(Guid subscriptionId, int monthsToRenew, CancellationToken cancellationToken);
    Task<List<SubscriptionDto>> GetUpcomingBills(string auth0Id, CancellationToken cancellationToken);
}
