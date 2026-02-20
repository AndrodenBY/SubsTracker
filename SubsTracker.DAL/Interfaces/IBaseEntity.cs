namespace SubsTracker.DAL.Interfaces;

public interface IBaseEntity
{
    Guid Id { get; set; }
    DateTime CreatedAt { get; set; }
    DateTime ModifiedAt { get; set; }
}
