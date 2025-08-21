namespace SubsTracker.BLL.DTOs;

public class GroupMemberDto: BaseDto
{
    public Guid UserId { get; set; }
    public Guid GroupId { get; set; }
}