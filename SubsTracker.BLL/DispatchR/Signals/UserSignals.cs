using DispatchR.Abstractions.Notification;

namespace SubsTracker.BLL.DispatchR.Signals;

public class UserSignals
{
    public record Created(string Auth0Id) : INotification;
    public record Deleted(string Auth0Id) : INotification;
    public record Updated(string Auth0Id) : INotification;
}
