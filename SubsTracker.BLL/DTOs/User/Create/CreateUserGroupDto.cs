namespace SubsTracker.BLL.DTOs.User.Create;

public class CreateUserGroupDto
{
    public Guid UserId { get; set; }
    public required string Name { get; set; }
}