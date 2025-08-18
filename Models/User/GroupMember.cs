namespace SubsTracker.Models;

public class GroupMember: BaseModel
{
    public Guid UserId { get; set; }
    public Guid GroupId { get; set; }
}