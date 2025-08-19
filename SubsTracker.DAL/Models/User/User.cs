namespace SubsTracker.DAL.Models.User;

public class User: BaseModel
{
    public string FirstName { get; set; } = string.Empty;
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public int SubscriptionCount { get; set; }
    public List<Subscription.Subscription>? Subscriptions { get; set; } = new();
    public List<UserGroup>? Groups { get; set; } = new();
}