using SubsTracker.Domain.Enums;
using SubsTracker.Domain.Interfaces;

namespace SubsTracker.BLL.DTOs.User.Update;

public class UpdateGroupMemberDto
{
    public Guid Id { get; set; }
    public MemberRole? Role { get; set; }
}