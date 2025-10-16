namespace SubsTracker.Messaging.Interfaces;

public interface IBaseEvent
{
    DateTime SendedAt { get; init; }
}
