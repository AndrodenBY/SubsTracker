namespace SubsTracker.IntegrationTests.DataSeedEntities;

public class UserGroupSeedEntity
{
    public UserModel User { get; set; } = null!;
    public Group Group { get; set; } = null!;
    public List<SubscriptionModel> Subscriptions { get; set; } = new();
    public List<GroupMember> Members { get; set; } = new();
}
