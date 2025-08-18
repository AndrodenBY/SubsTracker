
using System.Text.RegularExpressions;
using SubsTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace SubsTracker;

public class SubsDbContext : DbContext
{
    public DbSet<User> Users {get; set;}
    public DbSet<Subscription> Subscriptions {get; set;}
    public DbSet<UserGroup> UserGroups {get; set;}
    public DbSet<GroupMember> Members {get; set;}
    
    public SubsDbContext(DbContextOptions<SubsDbContext> options) : base(options)
    {
        Database.EnsureCreated();
    }
}