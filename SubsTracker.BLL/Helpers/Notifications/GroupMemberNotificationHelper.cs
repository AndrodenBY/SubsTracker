using SubsTracker.DAL.Entities.User;
using SubsTracker.Messaging.Contracts;

namespace SubsTracker.BLL.Helpers.Notifications;

public static class GroupMemberNotificationHelper
{
    public static MemberChangedRoleEvent CreateMemberChangedRoleEvent(GroupMember member)
    {
        return new MemberChangedRoleEvent(
            member.Id,
            member.GroupId,
            MemberRoleMapperHelper.ToMessagingRole(member.Role),
            member.Group.Name,
            member.UserEntity.Email
        );
    }

    public static MemberLeftGroupEvent CreateMemberLeftGroupEvent(GroupMember member)
    {
        return new MemberLeftGroupEvent(
            member.Id,
            member.GroupId,
            member.Group.Name,
            member.UserEntity.Email
        );
    }
}
