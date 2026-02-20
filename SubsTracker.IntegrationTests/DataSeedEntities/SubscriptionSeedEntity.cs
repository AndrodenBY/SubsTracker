using SubsTracker.DAL.Entities.Subscription;

namespace SubsTracker.IntegrationTests.DataSeedEntities;

public class SubscriptionSeedEntity
{
    public DAL.Entities.User.UserEntity UserEntity { get; set; } = null!;
    public List<SubscriptionEntity> Subscriptions { get; set; } = new();
}
