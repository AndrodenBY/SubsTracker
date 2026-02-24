namespace SubsTracker.DAL.Entities;

public class UserEntity : BaseEntity
{
    public string Auth0Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string? LastName { get; set; }
    public string Email { get; set; } = string.Empty;
    public List<SubscriptionEntity>? Subscriptions { get; set; } = new();
    public List<GroupEntity>? Groups { get; set; } = new();
}
