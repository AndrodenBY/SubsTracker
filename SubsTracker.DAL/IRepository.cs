namespace SubsTracker.DAL;

public interface IRepository<TModel> where TModel : class
{
    Task<IEnumerable<TModel>?> GetAllAsync();
    Task<TModel?> GetByIdAsync(Guid id);
    Task<bool> CreateAsync(TModel itemToCreate);
    Task<bool> UpdateAsync(TModel itemToUpdate);
    Task<bool> DeleteAsync(Guid id);
}