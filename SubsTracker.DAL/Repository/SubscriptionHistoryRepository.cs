using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SubsTracker.DAL.Entities;
using SubsTracker.DAL.Extensions;
using SubsTracker.DAL.Interfaces.Repositories;
using SubsTracker.Domain.Enums;
using SubsTracker.Domain.Pagination;

namespace SubsTracker.DAL.Repository;

public class SubscriptionHistoryRepository(SubsDbContext context)
    : Repository<SubscriptionHistory>(context), ISubscriptionHistoryRepository
{
    private readonly DbSet<SubscriptionHistory> _dbSet = context.Set<SubscriptionHistory>();

    public async Task<PaginatedList<SubscriptionHistory>> GetAllHistoryWithSubscriptions(
        Expression<Func<SubscriptionHistory, bool>>? predicate,
        PaginationParameters? paginationParameters,
        CancellationToken cancellationToken)
    {
        var query = _dbSet.AsNoTracking()
            .Include(nestedEntity => nestedEntity.Subscription)
            .AsQueryable();

        if (predicate is not null)
        {
            query = query.Where(predicate);
        }
        
        return await query
            .OrderByDescending(historyEntry => historyEntry.CreatedAt)
            .ToPagedListAsync(paginationParameters, cancellationToken);
    }

    public async Task<bool> Create(
        Guid subscriptionId, 
        SubscriptionAction action, 
        decimal? pricePaid, 
        CancellationToken cancellationToken)
    {
        var createHistoryItem = new SubscriptionHistory
        {
            SubscriptionId = subscriptionId,
            Action = action,
            PricePaid = pricePaid
        };
        
        await _dbSet.AddAsync(createHistoryItem, cancellationToken);
        return await Context.SaveChangesAsync(cancellationToken) > 0;
    }

    public async Task UpdateType(
        SubscriptionType originalType, 
        SubscriptionType updatedType,
        Guid subscriptionId, 
        decimal? price, 
        CancellationToken cancellationToken)
    {
        if (originalType != updatedType)
        {
            await Create(subscriptionId, SubscriptionAction.ChangeType, price, cancellationToken);
        }
    }
}
