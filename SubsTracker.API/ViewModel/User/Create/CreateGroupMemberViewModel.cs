using SubsTracker.Domain.Enums;

namespace SubsTracker.API.ViewModel.User.Create;

public class CreateGroupMemberViewModel
{
    public Guid UserId { get; set; }
    public Guid GroupId { get; set; }
    public MemberRole Role { get; set; }
}