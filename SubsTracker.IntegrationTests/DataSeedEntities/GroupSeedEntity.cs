using SubsTracker.DAL.Entities;

namespace SubsTracker.IntegrationTests.DataSeedEntities;

public class GroupSeedEntity
{
    public UserEntity UserEntity { get; set; } = null!;
    public GroupEntity GroupEntity { get; set; } = null!;
    public List<SubscriptionEntity> Subscriptions { get; set; } = new();
    public List<MemberEntity> Members { get; set; } = new();
}
