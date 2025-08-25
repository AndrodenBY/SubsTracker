using Microsoft.EntityFrameworkCore;
using SubsTracker.DAL.Models.Subscription;
using SubsTracker.DAL.Models.User;
using SubsTracker.Domain;
using SubsTracker.Domain.Interfaces;

namespace SubsTracker.DAL;

public class SubsDbContext : DbContext
{
    public DbSet<User> Users {get; set;}
    public DbSet<Subscription> Subscriptions {get; set;}
    public DbSet<UserGroup> UserGroups {get; set;}
    public DbSet<GroupMember> Members {get; set;}

    public SubsDbContext(DbContextOptions<SubsDbContext> options)
        : base(options)
    {
    }
    
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
        var entries = ChangeTracker.Entries<IBaseModel>();

        foreach (var entry in entries)
        {
            if (entry.State is EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
                entry.Entity.ModifiedAt = DateTime.UtcNow;
            }
            else if (entry.State is EntityState.Modified)
            {
                entry.Entity.ModifiedAt = DateTime.UtcNow;
            }
        }
    }
}