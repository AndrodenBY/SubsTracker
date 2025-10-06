namespace SubsTracker.IntegrationTests.DataSeedEntites;

public class UserGroupSeedEntity
{
    public UserModel User { get; set; }
    public Group Group { get; set; }
    public List<SubscriptionModel> Subscriptions { get; set; }
    public List<GroupMember> Members { get; set; }
}
