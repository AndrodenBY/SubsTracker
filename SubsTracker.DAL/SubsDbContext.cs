using Microsoft.EntityFrameworkCore;
using SubsTracker.DAL.Models.Subscription;
using SubsTracker.DAL.Models.User;
using SubsTracker.Domain.Interfaces;

namespace SubsTracker.DAL;

public class SubsDbContext(DbContextOptions<SubsDbContext> options) : DbContext(options)
{
    public DbSet<User> Users {get; set;}
    public DbSet<Subscription> Subscriptions {get; set;}
    public DbSet<SubscriptionHistory> SubscriptionHistory {get; set;}
    public DbSet<UserGroup> UserGroups {get; set;}
    public DbSet<GroupMember> Members {get; set;}
    
    public override int SaveChanges()
    {
        OnBeforeSaving();
        return base.SaveChanges();
    }
    
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        OnBeforeSaving();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void OnBeforeSaving()
    {
        var entries = ChangeTracker.Entries<IBaseModel>();

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.ModifiedAt = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.ModifiedAt = DateTime.UtcNow;
                    break;
                default:
                    break;
            }
        }
    }
}
