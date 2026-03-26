using SubsTracker.BLL.DTOs.Subscription;
using SubsTracker.BLL.Filter;
using SubsTracker.Domain.Pagination;

namespace SubsTracker.BLL.Interfaces
{
    public interface ISubscriptionService
    {
        Task<SubscriptionDto?> GetUserInfoById(Guid id, CancellationToken cancellationToken);
        Task<SubscriptionDto> GetById(Guid id, CancellationToken cancellationToken);
        Task<PaginatedList<SubscriptionDto>> GetAll(SubscriptionFilterDto? filter, PaginationParameters? paginationParameters, CancellationToken cancellationToken);
        Task<SubscriptionDto> Create(Guid userId, CreateSubscriptionDto createDto, CancellationToken cancellationToken);
        Task<SubscriptionDto> Update(Guid userId, UpdateSubscriptionDto updateDto, CancellationToken cancellationToken);
        Task<SubscriptionDto> CancelSubscription(Guid userId, Guid subscriptionId, CancellationToken cancellationToken);
        Task<SubscriptionDto> RenewSubscription(Guid subscriptionId, int monthsToRenew, CancellationToken cancellationToken);
        Task<List<SubscriptionDto>> GetUpcomingBills(Guid userId, CancellationToken cancellationToken);
    }
}
