namespace SubsTracker.BLL.DTOs.User.Update;

public class UpdateUserDto : BaseDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
}
