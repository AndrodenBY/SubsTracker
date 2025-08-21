using System.ComponentModel.DataAnnotations;
using SubsTracker.Domain.Enums;

namespace SubsTracker.BLL.DTOs.Subscription;

public class CreateSubscriptionDto
{
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    [Range(0.1, (double)decimal.MaxValue)]
    public decimal Price { get; set; }
    [Required]
    public DateOnly DueDate { get; set; }
    [Required]
    public SubscriptionType Type { get; set; }
    [Required]
    public SubscriptionContent Content { get; set; }
}