using SubsTracker.DAL.Entities;

namespace SubsTracker.IntegrationTests.DataSeedEntities;

public class SubscriptionSeedEntity
{
    public UserEntity UserEntity { get; set; } = null!;
    public List<SubscriptionEntity> Subscriptions { get; set; } = new();
}
