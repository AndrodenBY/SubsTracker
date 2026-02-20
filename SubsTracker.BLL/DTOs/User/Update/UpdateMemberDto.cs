using SubsTracker.Domain.Enums;

namespace SubsTracker.BLL.DTOs.User.Update;

public class UpdateMemberDto
{
    public Guid Id { get; set; }
    public MemberRole? Role { get; set; }
}
