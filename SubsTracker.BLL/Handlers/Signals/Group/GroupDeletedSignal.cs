using DispatchR.Abstractions.Notification;

namespace SubsTracker.BLL.Handlers.Signals.Group;

public record GroupDeletedSignal(Guid GroupId, Guid UserId) : INotification;
