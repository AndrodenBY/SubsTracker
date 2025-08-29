using SubsTracker.Domain.Enums;

namespace SubsTracker.API.ViewModel.User.Update;

public class UpdateGroupMemberViewModel
{
    public Guid Id { get; set; }
    public MemberRole? Role { get; set; }
}