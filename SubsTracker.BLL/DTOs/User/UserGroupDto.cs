namespace SubsTracker.BLL.DTOs;

public class UserGroupDto: BaseDto
{
    public string Name { get; set; } = string.Empty;
    public List<GroupMemberDto>? Members { get; set; }
}