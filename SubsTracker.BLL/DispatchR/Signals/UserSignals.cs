using DispatchR.Abstractions.Notification;

namespace SubsTracker.BLL.DispatchR.Signals;

public class UserSignals
{
    public record Created(string IdentityId) : INotification;
    public record Deleted(string IdentityId) : INotification;
    public record Updated(string IdentityId) : INotification;
}
