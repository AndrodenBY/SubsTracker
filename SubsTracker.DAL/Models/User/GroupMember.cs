using System.Text.RegularExpressions;
using SubsTracker.Domain.Enums;

namespace SubsTracker.DAL.Models.User;

public class GroupMember : BaseModel
{
    public Guid UserId { get; set; }
    public User User { get; set; }
    public Guid GroupId { get; set; }
    public Group Group { get; set; }
    public MemberRole Role { get; set; } = MemberRole.Member;
}