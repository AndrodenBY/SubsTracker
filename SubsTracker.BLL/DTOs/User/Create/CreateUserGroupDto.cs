namespace SubsTracker.BLL.DTOs.User.Create;

public class CreateUserGroupDto
{
    public required Guid UserId { get; set; }
    public required string Name { get; set; }
}
