using DispatchR.Abstractions.Notification;

namespace SubsTracker.BLL.Handlers.Signals.User;

public record UserCreatedSignal(string ExternalId) : INotification;
