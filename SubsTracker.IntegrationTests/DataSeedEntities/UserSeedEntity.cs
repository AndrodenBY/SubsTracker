using SubsTracker.DAL.Entities;

namespace SubsTracker.IntegrationTests.DataSeedEntities;

public class UserSeedEntity
{
    public UserEntity UserEntity { get; set; } = null!;
    public List<SubscriptionEntity> Subscriptions { get; set; } = new();
    public List<GroupEntity> UserGroups { get; set; } = new();
}
