using AutoMapper;
using SubsTracker.BLL.DTOs.Subscription;
using SubsTracker.BLL.Filter;
using SubsTracker.BLL.Helpers.Filters;
using SubsTracker.BLL.Interfaces;
using SubsTracker.DAL.Interfaces.Repositories;
using SubsTracker.Domain.Pagination;

namespace SubsTracker.BLL.Services;

public class SubscriptionHistoryService(
    ISubscriptionHistoryRepository subscriptionHistoryRepository,
    IMapper mapper)
    : ISubscriptionHistoryService
{
    public async Task<PaginatedList<SubscriptionHistoryDto>> GetAllHistory(Guid subscriptionId, SubscriptionHistoryFilter? filter, PaginationParameters? paginationParameters, CancellationToken cancellationToken)  
    {
        var expression = SubscriptionHistoryFilterHelper.CreatePredicate(filter);
        var pagedEntities = await subscriptionHistoryRepository.GetAll(expression, paginationParameters, cancellationToken);
        return pagedEntities.MapToPage(mapper.Map<SubscriptionHistoryDto>);
    }
}
