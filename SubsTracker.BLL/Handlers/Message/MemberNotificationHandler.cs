using DispatchR.Abstractions.Notification;
using SubsTracker.BLL.Handlers.Signals.Member;
using SubsTracker.BLL.Helpers.Notifications;
using SubsTracker.DAL.Interfaces.Repositories;
using SubsTracker.Messaging.Interfaces;

namespace SubsTracker.BLL.Handlers.Message;

public class MemberNotificationHandler(IGroupMemberRepository memberRepository, IMessageService messageService) 
    : INotificationHandler<MemberLeftSignal>,
        INotificationHandler<MemberChangedRoleSignal>
{
    public async ValueTask Handle(MemberLeftSignal signal, CancellationToken cancellationToken)
    {
        var messageEvent = GroupMemberNotificationHelper.CreateMemberLeftGroupEvent(
            signal.MemberId,
            signal.GroupId,
            signal.GroupName,
            signal.UserEmail);
        
        await messageService.NotifyMemberLeftGroup(messageEvent, cancellationToken);
    }

    public async ValueTask Handle(MemberChangedRoleSignal signal, CancellationToken cancellationToken)
    {
        var memberChangedRoleEvent = GroupMemberNotificationHelper.CreateMemberChangedRoleEvent(
            signal.MemberId,
            signal.GroupId,
            signal.GroupName,
            signal.UserEmail,
            signal.ChangedRole);

        await messageService.NotifyMemberChangedRole(memberChangedRoleEvent, cancellationToken);
    }
}
