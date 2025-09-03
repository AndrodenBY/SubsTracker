using System.Linq.Expressions;

namespace SubsTracker.Domain.Interfaces;

public interface IService<TEntity, TDto, TCreateDto, TUpdateDto, TFilterDto>
    where TEntity : IBaseModel
    where TDto : class
    where TCreateDto : class
    where TUpdateDto : class
    where TFilterDto : class
{
    Task<IEnumerable<TDto>> GetAll(Expression<Func<TEntity, bool>>? predicate, CancellationToken cancellationToken);
    Task<TDto?> GetById(Guid id, CancellationToken cancellationToken);
    Task<TDto> Create(TCreateDto entityToCreate, CancellationToken cancellationToken);
    Task<TDto> Update(Guid updateId, TUpdateDto entityToUpdate, CancellationToken cancellationToken);
    Task<bool> Delete(Guid id, CancellationToken cancellationToken);
}