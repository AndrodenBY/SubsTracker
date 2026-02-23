using SubsTracker.Domain.Enums;

namespace SubsTracker.BLL.DTOs.User.Create;

public class CreateMemberDto
{
    public Guid UserId { get; set; }
    public Guid GroupId { get; set; }
    public MemberRole Role { get; set; } = MemberRole.Participant;
}
