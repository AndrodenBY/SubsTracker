using System.Linq.Expressions;
using SubsTracker.DAL.Entities;
using SubsTracker.Domain.Enums;
using SubsTracker.Domain.Pagination;

namespace SubsTracker.DAL.Interfaces.Repositories;

public interface ISubscriptionHistoryRepository : IRepository<SubscriptionHistory>
{
    Task<PaginatedList<SubscriptionHistory>> GetAllHistoryWithSubscriptions(Expression<Func<SubscriptionHistory, bool>>? predicate, PaginationParameters? paginationParameters, CancellationToken cancellationToken);
    Task<bool> Create(Guid subscriptionId, SubscriptionAction action, decimal? pricePaid, CancellationToken cancellationToken);

    Task UpdateType(SubscriptionType originalType, SubscriptionType updatedType, Guid subscriptionId, decimal? price, CancellationToken cancellationToken);
}
