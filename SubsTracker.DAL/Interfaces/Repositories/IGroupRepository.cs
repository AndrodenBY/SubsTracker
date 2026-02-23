using SubsTracker.DAL.Entities;

namespace SubsTracker.DAL.Interfaces.Repositories;

public interface IGroupRepository : IRepository<GroupEntity>
{
    Task<GroupEntity?> GetFullInfoById(Guid id, CancellationToken cancellationToken);
}
