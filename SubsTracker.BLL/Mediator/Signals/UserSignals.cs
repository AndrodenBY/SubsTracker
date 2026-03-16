using DispatchR.Abstractions.Notification;

namespace SubsTracker.BLL.Mediator.Signals;

public class UserSignals
{
    public record Created(string IdentityId) : INotification;
    public record Deleted(Guid Id) : INotification;
    public record Updated(Guid Id) : INotification;
}
