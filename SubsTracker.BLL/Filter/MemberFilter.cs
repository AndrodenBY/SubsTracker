using SubsTracker.Domain.Enums;

namespace SubsTracker.BLL.Filter;

public class MemberFilter
{
    public Guid? Id { get; set; }
    public MemberRole? Role { get; set; }
}
