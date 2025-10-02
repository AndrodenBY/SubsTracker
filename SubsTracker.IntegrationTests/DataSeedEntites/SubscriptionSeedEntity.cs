namespace SubsTracker.IntegrationTests.DataSeedEntites;

public class SubscriptionSeedEntity
{
    public User User { get; set; } = null!;
    public List<DAL.Models.Subscription.Subscription> Subscriptions { get; set; } = new();
}
