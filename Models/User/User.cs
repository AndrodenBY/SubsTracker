namespace SubsTracker.Models;

public class User: BaseModel
{
    public string FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public int SubsCount { get; set; } = 0;
    //public List<Subscription>? Subs { get; set; } = new();
    public List<UserGroup>? Groups { get; set; } = new();
}

