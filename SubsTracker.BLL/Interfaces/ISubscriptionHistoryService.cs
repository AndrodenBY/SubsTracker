using SubsTracker.BLL.DTOs.Subscription;
using SubsTracker.BLL.Filter;
using SubsTracker.Domain.Pagination;

namespace SubsTracker.BLL.Interfaces;

public interface ISubscriptionHistoryService
{
    Task<PaginatedList<SubscriptionHistoryDto>> GetAllHistory(Guid subscriptionId, SubscriptionHistoryFilter? filter, PaginationParameters? paginationParameters, CancellationToken cancellationToken);
}
