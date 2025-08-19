namespace SubsTracker.Domain;

public interface IRepository<TModel> where TModel : IBaseModel
{
    Task<IEnumerable<TModel>?> GetAllAsync();
    Task<TModel?> GetByIdAsync(Guid id);
    Task<bool> CreateAsync(TModel itemToCreate);
    Task<bool> UpdateAsync(TModel itemToUpdate);
    Task<bool> DeleteAsync(Guid id);
}