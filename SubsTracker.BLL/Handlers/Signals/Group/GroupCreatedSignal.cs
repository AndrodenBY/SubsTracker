using DispatchR.Abstractions.Notification;

namespace SubsTracker.BLL.Handlers.Signals.Group;

public record GroupCreatedSignal(Guid GroupId, Guid UserId) : INotification;

