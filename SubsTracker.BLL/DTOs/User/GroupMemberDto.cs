using SubsTracker.Domain.Enums;

namespace SubsTracker.BLL.DTOs.User;

public class GroupMemberDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid GroupId { get; set; }
    public MemberRole Role { get; set; } = MemberRole.Participant;
}
