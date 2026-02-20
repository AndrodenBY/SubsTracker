using Microsoft.EntityFrameworkCore;
using SubsTracker.DAL.Entities.Subscription;
using SubsTracker.DAL.Interfaces.Repositories;

namespace SubsTracker.DAL.Repository;

public class SubscriptionRepository(SubsDbContext context) : Repository<SubscriptionEntity>(context), ISubscriptionRepository
{
    private readonly DbSet<SubscriptionEntity> _dbSet = context.Set<SubscriptionEntity>();

    public Task<SubscriptionEntity?> GetUserInfoById(Guid id, CancellationToken cancellationToken)
    {
        return _dbSet
            .AsSplitQuery()
            .Include(g => g.User)
            .FirstOrDefaultAsync(g => g.Id == id, cancellationToken);
    }

    public async Task<List<SubscriptionEntity>> GetUpcomingBills(Guid userId, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        var sevenDaysFromNow = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));

        return await _dbSet
            .Where(s => s.UserId == userId && s.DueDate >= today && s.DueDate <= sevenDaysFromNow)
            .OrderBy(s => s.DueDate)
            .ToListAsync(cancellationToken);
    }
}
