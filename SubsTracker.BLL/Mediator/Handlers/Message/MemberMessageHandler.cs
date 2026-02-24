using DispatchR.Abstractions.Notification;
using SubsTracker.BLL.Helpers.Messages;
using SubsTracker.BLL.Mediator.Signals;
using SubsTracker.Messaging.Interfaces;

namespace SubsTracker.BLL.Mediator.Handlers.Message;


public class MemberNotificationHandler(IMessageService messageService)  
    : INotificationHandler<MemberSignals.Left>,
      INotificationHandler<MemberSignals.ChangedRole>
{
    public async ValueTask Handle(MemberSignals.Left signal, CancellationToken cancellationToken)
    {
        var messageEvent = MemberMessageHelper.CreateMemberLeftGroupEvent(
            signal.MemberId,
            signal.GroupId,
            signal.GroupName,
            signal.UserEmail);
        
        await messageService.NotifyMemberLeftGroup(messageEvent, cancellationToken);
    }

    public async ValueTask Handle(MemberSignals.ChangedRole signal, CancellationToken cancellationToken)
    {
        var memberChangedRoleEvent = MemberMessageHelper.CreateMemberChangedRoleEvent(
            signal.MemberId,
            signal.GroupId,
            signal.GroupName,
            signal.UserEmail,
            signal.NewRole);

        await messageService.NotifyMemberChangedRole(memberChangedRoleEvent, cancellationToken);
    }
}
