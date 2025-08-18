using SubsTracker.Enums;

namespace SubsTracker.Models;

public class Subscription: BaseModel
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    public DateOnly DueDate {get; set;}
    public SubscriptionType Type { get; set; }
    public SubscriptionContent Content { get; set; }
    
}