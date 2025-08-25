using System.ComponentModel.DataAnnotations;
using SubsTracker.Domain.Enums;

namespace SubsTracker.BLL.DTOs.Subscription;

public class UpdateSubscriptionDto : BaseDto
{
    public string Name { get; set; } = string.Empty;
    public decimal? Price { get; set; }
    public DateOnly? DueDate { get; set; }
    public SubscriptionType Type { get; set; }
    public SubscriptionContent Content { get; set; }
}