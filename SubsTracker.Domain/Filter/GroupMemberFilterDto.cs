using SubsTracker.Domain.Enums;

namespace SubsTracker.BLL.DTOs.Filter;

public class GroupMemberFilterDto
{
    public Guid? UserId { get; set; }
    public Guid? GroupId { get; set; }
    public MemberRole? Role { get; set; }
}