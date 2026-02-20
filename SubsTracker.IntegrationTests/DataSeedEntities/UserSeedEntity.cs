using SubsTracker.DAL.Entities.Subscription;

namespace SubsTracker.IntegrationTests.DataSeedEntities;

public class UserSeedEntity
{
    public DAL.Entities.User.UserEntity UserEntity { get; set; } = null!;
    public List<SubscriptionEntity> Subscriptions { get; set; } = new();
    public List<DAL.Entities.User.UserGroup> UserGroups { get; set; } = new();
}
