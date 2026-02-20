namespace SubsTracker.IntegrationTests.DataSeedEntities;

public class SubscriptionSeedEntity
{
    public UserModel UserEntity { get; set; } = null!;
    public List<SubscriptionModel> Subscriptions { get; set; } = new();
}
