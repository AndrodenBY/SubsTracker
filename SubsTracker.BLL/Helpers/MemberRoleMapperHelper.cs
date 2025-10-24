namespace SubsTracker.BLL.Helpers;

public static class MemberRoleMapperHelper
{
    public static Messaging.Enums.MemberRole ToMessagingRole(Domain.Enums.MemberRole domainRole)
    {
        return domainRole switch
        {
            Domain.Enums.MemberRole.Participant => Messaging.Enums.MemberRole.Participant,
            Domain.Enums.MemberRole.Moderator => Messaging.Enums.MemberRole.Moderator,
            Domain.Enums.MemberRole.Admin => Messaging.Enums.MemberRole.Admin,
            _ => throw new ArgumentOutOfRangeException(nameof(domainRole), domainRole, "Unknown member role")
        };
    }
}
