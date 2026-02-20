using SubsTracker.DAL.Entities.Subscription;
using SubsTracker.DAL.Entities.User;

namespace SubsTracker.IntegrationTests.DataSeedEntities;

public class UserGroupSeedEntity
{
    public DAL.Entities.User.UserEntity UserEntity { get; set; } = null!;
    public DAL.Entities.User.UserGroup Group { get; set; } = null!;
    public List<SubscriptionEntity> Subscriptions { get; set; } = new();
    public List<GroupMember> Members { get; set; } = new();
}
