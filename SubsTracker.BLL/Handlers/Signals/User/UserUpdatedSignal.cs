using DispatchR.Abstractions.Notification;

namespace SubsTracker.BLL.Handlers.Signals.User;

public record UserUpdatedSignal(string ExternalId) : INotification;
