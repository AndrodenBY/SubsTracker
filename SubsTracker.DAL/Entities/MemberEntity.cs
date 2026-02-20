using SubsTracker.Domain.Enums;

namespace SubsTracker.DAL.Entities;

public class MemberEntity : BaseEntity
{
    public required Guid UserId { get; set; }
    public required UserEntity User { get; set; }
    public required Guid GroupId { get; set; }
    public required GroupEntity Group { get; set; }
    public MemberRole Role { get; set; } = MemberRole.Participant;
}
