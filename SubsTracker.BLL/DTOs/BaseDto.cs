using SubsTracker.Domain.Interfaces;

namespace SubsTracker.BLL.DTOs;

public class BaseDto : IBaseDto
{
    public Guid Id { get; set; }
}
