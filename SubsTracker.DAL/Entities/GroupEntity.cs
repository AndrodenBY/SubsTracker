namespace SubsTracker.DAL.Entities;

public class GroupEntity : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public Guid? UserId { get; set; }
    public UserEntity? User { get; set; }
    public List<MemberEntity>? Members { get; set; } = new();
    public List<SubscriptionEntity>? SharedSubscriptions { get; set; } = new();
}
