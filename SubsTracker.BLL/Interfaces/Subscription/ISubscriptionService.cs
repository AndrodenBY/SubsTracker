using SubsTracker.BLL.DTOs.Subscription;
using SubsTracker.Domain.Filter;
using SubscriptionModel = SubsTracker.DAL.Models.Subscription.Subscription;

namespace SubsTracker.BLL.Interfaces.Subscription;

public interface ISubscriptionService : 
    IService<SubscriptionModel, SubscriptionDto, CreateSubscriptionDto, UpdateSubscriptionDto, SubscriptionFilterDto>
{
    Task<IEnumerable<SubscriptionDto>> GetAll(SubscriptionFilterDto? filter, CancellationToken cancellationToken);
    Task<SubscriptionDto> Create(Guid userId, CreateSubscriptionDto createDto, CancellationToken cancellationToken);
    Task<SubscriptionDto> RenewSubscription(Guid subscriptionId, int monthsToRenew, CancellationToken cancellationToken);
    Task<IEnumerable<SubscriptionDto>> GetUpcomingBills(Guid userId, CancellationToken cancellationToken);
}
