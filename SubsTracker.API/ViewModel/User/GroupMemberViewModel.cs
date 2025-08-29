using SubsTracker.Domain.Enums;

namespace SubsTracker.API.ViewModel.User;

public class GroupMemberViewModel
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid GroupId { get; set; }
    public MemberRole Role { get; set; } = MemberRole.Member;
}