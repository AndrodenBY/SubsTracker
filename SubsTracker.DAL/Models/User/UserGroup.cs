namespace SubsTracker.DAL.Models.User;

public class UserGroup : BaseModel
{
    public string Name { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public User User { get; set; }
    public List<GroupMember>? Members { get; set; }
    public List<Subscription.Subscription>? SharedSubscriptions { get; set; }
}