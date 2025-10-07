namespace SubsTracker.IntegrationTests.DataSeedEntites;

public class UserSeedEntity
{
    public UserModel User { get; set; } = null!;
    public List<SubscriptionModel> Subscriptions { get; set; } = new();
    public List<Group> UserGroups { get; set; } = new();
}
