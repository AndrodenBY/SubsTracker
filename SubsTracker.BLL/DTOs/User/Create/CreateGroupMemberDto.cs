using SubsTracker.Domain.Enums;

namespace SubsTracker.BLL.DTOs.User.Create;

public class CreateGroupMemberDto
{
    public Guid UserId { get; set; }
    public Guid GroupId { get; set; }
    public MemberRole Role { get; set; } = MemberRole.Participant;
}