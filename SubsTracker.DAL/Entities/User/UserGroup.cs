namespace SubsTracker.DAL.Entities.User;

public class UserGroup : BaseModel
{
    public string Name { get; set; } = string.Empty;
    public Guid? UserId { get; set; }
    public UserEntity? User { get; set; }
    public List<GroupMember>? Members { get; set; } = new();
    public List<Subscription.SubscriptionEntity>? SharedSubscriptions { get; set; } = new();
}
