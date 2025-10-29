using SubsTracker.Domain.Enums;

namespace SubsTracker.DAL.Models.User;

public class GroupMember : BaseModel
{
    public required Guid UserId { get; set; }
    public required User User { get; set; }
    public required Guid GroupId { get; set; }
    public required UserGroup Group { get; set; }
    public MemberRole Role { get; set; } = MemberRole.Participant;
}