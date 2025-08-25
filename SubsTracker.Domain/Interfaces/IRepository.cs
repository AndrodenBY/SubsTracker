using System.Linq.Expressions;

namespace SubsTracker.Domain.Interfaces;

public interface IRepository<TModel> where TModel : IBaseModel
{
    Task<IEnumerable<TModel>> GetAll(CancellationToken cancellationToken = default);
    Task<TModel?> GetById(Guid id, CancellationToken cancellationToken = default);
    Task<TModel> Create(TModel entityToCreate, CancellationToken cancellationToken = default);
    Task<TModel> Update(TModel entityToUpdate, CancellationToken cancellationToken = default);
    Task<bool> Delete(Guid id, CancellationToken cancellationToken = default);
    Task<TModel?> FindByCondition(Expression<Func<TModel, bool>> predicate, CancellationToken cancellationToken = default);
}