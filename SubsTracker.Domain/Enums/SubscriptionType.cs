using System.Text.Json.Serialization;

namespace SubsTracker.Domain.Enums;

public enum SubscriptionType
{
    None = 0,
    Free = 1,
    Lifetime = 2,
    Enterprise = 3,
    Trial = 4,
    Family = 5,
    Student = 6,
    Standard = 7
}
