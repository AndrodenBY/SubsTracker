using SubsTracker.Domain.Enums;
using SubsTracker.Messaging.Contracts;

namespace SubsTracker.BLL.Helpers.Messages;

public static class MemberMessageHelper
{
    public static MemberChangedRoleEvent CreateMemberChangedRoleEvent(Guid memberId, Guid groupId, string groupName, string email, MemberRole memberRole)
    {
        return new MemberChangedRoleEvent(
            memberId,
            groupId,
            MemberRoleMapperHelper.ToMessagingRole(memberRole),
            groupName,
            email
        );
    }

    public static MemberLeftGroupEvent CreateMemberLeftGroupEvent(Guid memberId, Guid groupId, string groupName, string email)
    {
        return new MemberLeftGroupEvent(
            memberId,
            groupId,
            groupName,
            email
        );
    }
}
