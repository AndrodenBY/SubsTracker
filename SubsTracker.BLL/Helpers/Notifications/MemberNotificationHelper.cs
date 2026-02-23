using System.Diagnostics.CodeAnalysis;
using SubsTracker.DAL.Entities;
using SubsTracker.Messaging.Contracts;

namespace SubsTracker.BLL.Helpers.Notifications;

[ExcludeFromCodeCoverage]
public static class MemberNotificationHelper
{
    public static MemberChangedRoleEvent CreateMemberChangedRoleEvent(MemberEntity memberEntity)
    {
        return new MemberChangedRoleEvent(
            memberEntity.Id,
            memberEntity.GroupId,
            MemberRoleMapperHelper.ToMessagingRole(memberEntity.Role),
            memberEntity.Group.Name,
            memberEntity.User.Email
        );
    }

    public static MemberLeftGroupEvent CreateMemberLeftGroupEvent(MemberEntity memberEntity)
    {
        return new MemberLeftGroupEvent(
            memberEntity.Id,
            memberEntity.GroupId,
            memberEntity.Group.Name,
            memberEntity.User.Email
        );
    }
}
