using DispatchR.Abstractions.Notification;

namespace SubsTracker.BLL.Mediator.Signals;

public class GroupSignals
{
    public record Created(Guid GroupId, Guid UserId) : INotification;
    public record Updated(Guid GroupId, Guid UserId) : INotification;
    public record Deleted(Guid GroupId, Guid UserId) : INotification;
}
