namespace SubsTracker.DAL.Models.User;

public class UserGroup: BaseModel
{
    public string Name { get; set; } = string.Empty;
    public List<GroupMember>? Members { get; set; }
}