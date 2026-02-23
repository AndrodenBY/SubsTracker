using SubsTracker.DAL.Interfaces;

namespace SubsTracker.DAL.Entities;

public abstract class BaseEntity : IBaseEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
}
