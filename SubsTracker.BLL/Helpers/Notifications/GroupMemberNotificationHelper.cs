using SubsTracker.DAL.Entities;
using SubsTracker.Messaging.Contracts;

namespace SubsTracker.BLL.Helpers.Notifications;

public static class GroupMemberNotificationHelper
{
    public static MemberChangedRoleEvent CreateMemberChangedRoleEvent(MemberEntity memberEntity)
    {
        return new MemberChangedRoleEvent(
            memberEntity.Id,
            memberEntity.GroupId,
            MemberRoleMapperHelper.ToMessagingRole(memberEntity.Role),
            memberEntity.GroupEntity.Name,
            memberEntity.UserEntity.Email
        );
    }

    public static MemberLeftGroupEvent CreateMemberLeftGroupEvent(MemberEntity memberEntity)
    {
        return new MemberLeftGroupEvent(
            memberEntity.Id,
            memberEntity.GroupId,
            memberEntity.GroupEntity.Name,
            memberEntity.UserEntity.Email
        );
    }
}
