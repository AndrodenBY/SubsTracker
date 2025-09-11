namespace SubsTracker.DAL.Interfaces;

public interface IBaseModel
{
     Guid Id { get; set; }
     DateTime CreatedAt { get; set; }
     DateTime ModifiedAt { get; set; }
}
