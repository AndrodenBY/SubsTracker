namespace SubsTracker.Domain.Interfaces;

public interface IService<TEntity, TDto, TCreateDto, TUpdateDto>
    where TEntity : IBaseModel
    where TDto : class
    where TCreateDto : class
    where TUpdateDto : class
{
    Task<IEnumerable<TDto>> GetAll(CancellationToken cancellationToken = default);
    Task<TDto?> GetById(Guid id, CancellationToken cancellationToken = default);
    Task<TDto> Create(TCreateDto entityToCreate, CancellationToken cancellationToken = default);
    Task<TDto> Update(TUpdateDto entityToUpdate, CancellationToken cancellationToken = default);
    Task<bool> Delete(Guid id, CancellationToken cancellationToken = default);
}