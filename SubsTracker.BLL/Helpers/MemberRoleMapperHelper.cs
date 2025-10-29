using SubsTracker.Messaging.Enums;

namespace SubsTracker.BLL.Helpers;

public static class MemberRoleMapperHelper
{
    public static MemberRole ToMessagingRole(Domain.Enums.MemberRole domainRole)
    {
        return domainRole switch
        {
            Domain.Enums.MemberRole.Participant => MemberRole.Participant,
            Domain.Enums.MemberRole.Moderator => MemberRole.Moderator,
            Domain.Enums.MemberRole.Admin => MemberRole.Admin,
            _ => throw new ArgumentOutOfRangeException(nameof(domainRole), domainRole, "Unknown member role")
        };
    }
}
