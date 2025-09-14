using SubsTracker.Domain.Enums;

namespace SubsTracker.DAL.Models.Subscription;

public class Subscription : BaseModel
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; } 
    public Guid? UserId { get; set; }
    public User.User? User { get; set; }
    public DateOnly DueDate {get; set;}
    public bool Active {get; set;} = true;
    public SubscriptionType Type { get; set; }
    public SubscriptionContent Content { get; set; }
    public IEnumerable<SubscriptionHistory> History { get; set; }
}
