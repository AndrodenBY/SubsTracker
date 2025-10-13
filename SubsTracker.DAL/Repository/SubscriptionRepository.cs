using Microsoft.EntityFrameworkCore;
using SubsTracker.DAL.Interfaces.Repositories;
using SubsTracker.DAL.Models.Subscription;


namespace SubsTracker.DAL.Repository;

public class SubscriptionRepository(SubsDbContext context) : Repository<Subscription>(context), ISubscriptionRepository
{
    private readonly DbSet<Subscription> _dbSet = context.Set<Subscription>();

    public async Task<List<Subscription>> GetUpcomingBills(Guid userId, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        var sevenDaysFromNow = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));

        return await _dbSet
            .Where(s => s.UserId == userId && s.DueDate >= today && s.DueDate <= sevenDaysFromNow)
            .OrderBy(s => s.DueDate)
            .ToListAsync(cancellationToken);
    }
}
