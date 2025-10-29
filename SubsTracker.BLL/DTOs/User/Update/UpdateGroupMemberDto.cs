using SubsTracker.Domain.Enums;

namespace SubsTracker.BLL.DTOs.User.Update;

public class UpdateGroupMemberDto
{
    public Guid Id { get; set; }
    public MemberRole? Role { get; set; }
}