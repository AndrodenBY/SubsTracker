using SubsTracker.Domain.Enums;

namespace SubsTracker.Domain.Filter;

public class GroupMemberFilter
{
    public Guid? UserId { get; set; }
    public Guid? GroupId { get; set; }
    public MemberRole? Role { get; set; }
}