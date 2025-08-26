using System.Linq.Expressions;

namespace SubsTracker.Domain.Interfaces;

public interface IRepository<TModel> where TModel : IBaseModel
{
    Task<IEnumerable<TModel>> GetAll(CancellationToken cancellationToken);
    Task<TModel?> GetById(Guid id, CancellationToken cancellationToken);
    Task<TModel> Create(TModel entityToCreate, CancellationToken cancellationToken);
    Task<TModel> Update(TModel entityToUpdate, CancellationToken cancellationToken);
    Task<bool> Delete(Guid id, CancellationToken cancellationToken);
    Task<TModel?> GetByPredicate(Expression<Func<TModel, bool>> predicate, CancellationToken cancellationToken);
}