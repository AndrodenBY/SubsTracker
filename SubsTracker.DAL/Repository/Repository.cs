using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SubsTracker.DAL.Interfaces;
using SubsTracker.DAL.Interfaces.Repositories;
using SubsTracker.Domain.Pagination;

namespace SubsTracker.DAL.Repository;

public class Repository<TEntity>(SubsDbContext context) : IRepository<TEntity> where TEntity : class, IBaseEntity
{
    private readonly DbSet<TEntity> _dbSet = context.Set<TEntity>();
    protected readonly SubsDbContext Context = context;

    public async Task<PaginatedList<TEntity>> GetAll(
        Expression<Func<TEntity, bool>>? predicate,
        PaginationParameters? paginationParameters,
        CancellationToken cancellationToken)
    {
        var query = predicate is not null
            ? _dbSet.Where(predicate)
            : _dbSet;
        
        var count = await query.CountAsync(cancellationToken);
        query = query.OrderBy(entity => entity.Id);
        
        if (paginationParameters is not null)
        {
            query = query
                .Skip((paginationParameters.PageNumber - 1) * paginationParameters.PageSize)
                .Take(paginationParameters.PageSize);
        }

        var list = await query.AsNoTracking().ToListAsync(cancellationToken);

        var pageNumber = paginationParameters?.PageNumber ?? 1;
        var pageSize = paginationParameters?.PageSize ?? count;
        
        return list.ToPagedList(pageNumber, pageSize, count);
    }

    public virtual Task<TEntity?> GetById(Guid id, CancellationToken cancellationToken)
    {
        return _dbSet.FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);
    }

    public async Task<TEntity> Create(TEntity entityToCreate, CancellationToken cancellationToken)
    {
        _dbSet.Add(entityToCreate);
        await Context.SaveChangesAsync(cancellationToken);
        return entityToCreate;
    }

    public async Task<TEntity> Update(TEntity entityToUpdate, CancellationToken cancellationToken)
    {
        var existingEntity = Context.Update(entityToUpdate);
        await Context.SaveChangesAsync(cancellationToken);
        
        return existingEntity.Entity;
    }

    public async Task<bool> Delete(TEntity entityToDelete, CancellationToken cancellationToken)
    {
        _dbSet.Remove(entityToDelete);
        return await Context.SaveChangesAsync(cancellationToken) > 0;
    }

    public Task<TEntity?> GetByPredicate(Expression<Func<TEntity, bool>> expression, CancellationToken cancellationToken)
    {
        return _dbSet.FirstOrDefaultAsync(expression, cancellationToken);
    }
}
