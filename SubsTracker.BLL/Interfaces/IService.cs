using System.Linq.Expressions;
using SubsTracker.DAL.Interfaces;

namespace SubsTracker.BLL.Interfaces;

public interface IService<TEntity, TDto, in TCreateDto, in TUpdateDto, TFilterDto>
    where TEntity : IBaseModel
    where TDto : class
    where TCreateDto : class
    where TUpdateDto : class
    where TFilterDto : class
{
    Task<List<TDto>> GetAll(Expression<Func<TEntity, bool>>? predicate, CancellationToken cancellationToken);
    Task<TDto> GetById(Guid id, CancellationToken cancellationToken);
    Task<TDto> Create(TCreateDto createDto, CancellationToken cancellationToken);
    Task<TDto> Update(Guid updateId, TUpdateDto updateDto, CancellationToken cancellationToken);
    Task<bool> Delete(Guid id, CancellationToken cancellationToken);
    Task<TDto?> GetByPredicate(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken);
}
