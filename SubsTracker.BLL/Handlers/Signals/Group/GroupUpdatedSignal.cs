using DispatchR.Abstractions.Notification;

namespace SubsTracker.BLL.Handlers.Signals.Group;

public record GroupUpdatedSignal(Guid GroupId, Guid UserId) : INotification;
