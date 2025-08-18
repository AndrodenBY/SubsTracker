namespace SubsTracker.DAL.Models;

public class UserGroup: BaseModel
{
    public string Name { get; set; }
    public List<GroupMember>? Members { get; set; }
}