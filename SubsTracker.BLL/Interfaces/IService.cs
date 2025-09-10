using SubsTracker.DAL.Interfaces;

namespace SubsTracker.BLL.Interfaces;

public interface IService<TEntity, TDto, TCreateDto, TUpdateDto>
    where TEntity : IBaseModel
    where TDto : class
    where TCreateDto : class
    where TUpdateDto : class
{
    Task<IEnumerable<TDto>> GetAll(CancellationToken cancellationToken);
    Task<TDto?> GetById(Guid id, CancellationToken cancellationToken);
    Task<TDto> Create(TCreateDto createDto, CancellationToken cancellationToken);
    Task<TDto> Update(Guid updateId, TUpdateDto updateDto, CancellationToken cancellationToken);
    Task<bool> Delete(Guid id, CancellationToken cancellationToken);
}
