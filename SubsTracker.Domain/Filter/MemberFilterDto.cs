using SubsTracker.Domain.Enums;

namespace SubsTracker.Domain.Filter;

public class MemberFilterDto
{
    public Guid? Id { get; set; }
    public MemberRole? Role { get; set; }
}
