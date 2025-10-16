using SubsTracker.Messaging.Interfaces;

namespace SubsTracker.Messaging.Contracts;

public record BaseEvent : IBaseEvent
{
    public DateTime SendedAt { get; init; } = DateTime.Now;
}
