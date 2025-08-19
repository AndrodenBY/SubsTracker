namespace SubsTracker.DAL.Models.User;

public class User: BaseModel
{
    public string FirstName { get; set; } = string.Empty;
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public int SubsCount { get; set; }
    //public List<Subscription.Subscription>? Subs { get; set; } = new();
    public List<UserGroup>? Groups { get; set; } = new();
}