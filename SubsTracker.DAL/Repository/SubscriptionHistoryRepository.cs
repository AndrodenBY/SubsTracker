using Microsoft.EntityFrameworkCore;
using SubsTracker.DAL.Interfaces.Repositories;
using SubsTracker.DAL.Models.Subscription;
using SubsTracker.Domain.Enums;

namespace SubsTracker.DAL.Repository;

public class SubscriptionHistoryRepository(SubsDbContext context)
    : Repository<SubscriptionHistory>(context), ISubscriptionHistoryRepository
{
    private readonly DbSet<SubscriptionHistory> _dbSet = context.Set<SubscriptionHistory>();

    public async Task<bool> Create(Guid subscriptionId, SubscriptionAction action,
        decimal? pricePaid, CancellationToken cancellationToken)
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

    public async Task UpdateType(SubscriptionType originalType, SubscriptionType updatedType,
        Guid subscriptionId, decimal? price, CancellationToken cancellationToken)
    {
        if (originalType != updatedType)
            await Create(subscriptionId, SubscriptionAction.ChangeType, price, cancellationToken);
    }
}