using DispatchR.Abstractions.Notification;

namespace SubsTracker.BLL.Handlers.Signals.User;

public record UserDeletedSignal(string ExternalId) : INotification;
