using SubsTracker.Domain.Enums;

namespace SubsTracker.API.ViewModel;

public class MemberViewModel
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid GroupId { get; set; }
    public MemberRole Role { get; set; } = MemberRole.Participant;
}
