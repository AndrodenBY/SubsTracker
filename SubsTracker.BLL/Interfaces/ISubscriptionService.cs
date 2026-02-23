using SubsTracker.BLL.DTOs.Subscription;
using SubsTracker.DAL.Entities;
using SubsTracker.Domain.Filter;
using SubsTracker.Domain.Pagination;

namespace SubsTracker.BLL.Interfaces
{
    public interface ISubscriptionService : IService<SubscriptionEntity, SubscriptionDto, CreateSubscriptionDto, UpdateSubscriptionDto, SubscriptionFilterDto>
    {
        Task<SubscriptionDto?> GetUserInfoById(Guid id, CancellationToken cancellationToken);
        Task<PaginatedList<SubscriptionDto>> GetAll(SubscriptionFilterDto? filter, PaginationParameters? paginationParameters, CancellationToken cancellationToken);
        Task<SubscriptionDto> Create(string auth0Id, CreateSubscriptionDto createDto, CancellationToken cancellationToken);
        Task<SubscriptionDto> Update(string auth0Id, UpdateSubscriptionDto updateDto, CancellationToken cancellationToken);
        Task<SubscriptionDto> CancelSubscription(string auth0Id, Guid subscriptionId, CancellationToken cancellationToken);
        Task<SubscriptionDto> RenewSubscription(Guid subscriptionId, int monthsToRenew, CancellationToken cancellationToken);
        Task<List<SubscriptionDto>> GetUpcomingBills(string auth0Id, CancellationToken cancellationToken);
    }
}
