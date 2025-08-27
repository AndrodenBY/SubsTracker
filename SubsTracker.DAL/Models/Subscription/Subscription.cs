using SubsTracker.Domain.Enums;

namespace SubsTracker.DAL.Models.Subscription;

public class Subscription : BaseModel
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public DateOnly DueDate {get; set;}
    public SubscriptionType Type { get; set; }
    public SubscriptionContent Content { get; set; }
}