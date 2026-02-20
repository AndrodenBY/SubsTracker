using SubsTracker.DAL.Entities;

namespace SubsTracker.IntegrationTests.DataSeedEntities;

public class UserGroupSeedEntity
{
    public UserModel UserEntity { get; set; } = null!;
    public Group GroupEntity { get; set; } = null!;
    public List<SubscriptionModel> Subscriptions { get; set; } = new();
    public List<MemberEntity> Members { get; set; } = new();
}
