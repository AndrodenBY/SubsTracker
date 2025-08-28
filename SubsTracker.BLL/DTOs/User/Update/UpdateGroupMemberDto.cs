using SubsTracker.Domain.Enums;

namespace SubsTracker.BLL.DTOs.User.Update;

public class UpdateGroupMemberDto
{
    public MemberRole? Role { get; set; }
}