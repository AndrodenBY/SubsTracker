using System.Linq.Expressions;

namespace SubsTracker.DAL.Interfaces.Repositories;

public interface IRepository<TEntity> where TEntity : IBaseModel
{
    Task<IEnumerable<TEntity>> GetAll(Expression<Func<TEntity, bool>>? predicate, CancellationToken cancellationToken);
    Task<TEntity?> GetById(Guid id, CancellationToken cancellationToken);
    Task<TEntity> Create(TEntity entityToCreate, CancellationToken cancellationToken);
    Task<TEntity> Update(TEntity entityToUpdate, CancellationToken cancellationToken);
    Task<bool> Delete(TEntity entityToDelete, CancellationToken cancellationToken);
    Task<TEntity?> GetByPredicate(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken);
}
