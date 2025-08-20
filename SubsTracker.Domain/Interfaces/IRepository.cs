namespace SubsTracker.Domain;

public interface IRepository<TModel> where TModel : IBaseModel
{
    Task<IEnumerable<TModel>?> GetAllAsync();
    Task<TModel?> GetByIdAsync(Guid id);
    Task<bool> CreateAsync(TModel entityToCreate);
    Task<bool> UpdateAsync(TModel entityToUpdate);
    Task<bool> DeleteAsync(Guid id);
}