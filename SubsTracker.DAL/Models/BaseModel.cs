using SubsTracker.DAL.Interfaces;

namespace SubsTracker.DAL.Models;

public abstract class BaseModel : IBaseModel
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
}