using SubsTracker.BLL.DTOs.User.Create;

namespace SubsTracker.BLL.Helpers.Policy;

public interface IMemberPolicyChecker
{
    Task EnsureCanJoinGroup(CreateMemberDto createDto, CancellationToken cancellationToken);
}
