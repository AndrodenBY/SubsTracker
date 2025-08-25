using SubsTracker.Domain.Enums;

namespace SubsTracker.BLL.DTOs;

public class GroupMemberDto : BaseDto
{
    public Guid UserId { get; set; }
    public Guid GroupId { get; set; }
    public MemberRole Role { get; set; } = MemberRole.Member;
}