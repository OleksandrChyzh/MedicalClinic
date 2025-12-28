using System.Linq.Expressions;

namespace DAL.Interfaces;

public interface IBaseRepository<TEntity> where TEntity : class
{
    Task<TEntity?> GetByIdAsync(int id);

    Task AddAsync(TEntity entity);

    Task DeleteAsync(TEntity entity);

    Task DeleteByIdAsync(int id);

    Task UpdateAsync(TEntity entity);

    Task<IEnumerable<TEntity>> GetAllAsync(
        Expression<Func<TEntity, bool>>? filter = null,
        params Expression<Func<TEntity, object>>[] includes);

    Task<TEntity?> GetFirstOrDefaultAsync(
        Expression<Func<TEntity, bool>>? filter = null,
        params Expression<Func<TEntity, object>>[] includes);
}
