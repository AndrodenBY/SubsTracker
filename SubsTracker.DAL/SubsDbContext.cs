using Microsoft.EntityFrameworkCore;
using SubsTracker.DAL.Entities;
using SubsTracker.DAL.Interfaces;

namespace SubsTracker.DAL;

public class SubsDbContext : DbContext
{
    public SubsDbContext(DbContextOptions<SubsDbContext> options) : base(options)
    {
        
    }

    public DbSet<UserEntity> Users { get; set; }
    public DbSet<SubscriptionEntity> Subscriptions { get; set; }
    public DbSet<SubscriptionHistory> SubscriptionHistory { get; set; }
    public DbSet<GroupEntity> UserGroups { get; set; }
    public DbSet<MemberEntity> Members { get; set; }

    public override int SaveChanges()
    {
        OnBeforeSaving();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        OnBeforeSaving();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void OnBeforeSaving()
    {
        var entries = ChangeTracker.Entries<IBaseEntity>();

        foreach (var entry in entries)
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.ModifiedAt = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.ModifiedAt = DateTime.UtcNow;
                    break;
            }
    }
}
