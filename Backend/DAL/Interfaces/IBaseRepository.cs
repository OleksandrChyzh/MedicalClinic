namespace DAL.Interfaces;

public interface IBaseRepository<TEntity> where TEntity : class
{
    Task<IEnumerable<TEntity>> GetAllAsync();

    Task<TEntity?> GetByIdAsync(int id);

    Task AddAsync(TEntity entity);

    Task DeleteAsync(TEntity entity);

    Task DeleteByIdAsync(int id);

    Task UpdateAsync(TEntity entity);
}
