namespace SubsTracker.IntegrationTests.DataSeedEntites;

public class SubscriptionSeedEntity
{
    public UserModel User { get; set; } = null!;
    public List<SubscriptionModel> Subscriptions { get; set; } = new();
}
