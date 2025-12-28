using DAL.Data;
using DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories;

public class BaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : class
{
    protected AppDbContext Context { get; }
    protected DbSet<TEntity> DbSet { get; }

    public BaseRepository(AppDbContext context)
    {
        this.Context = context;
        this.DbSet = context.Set<TEntity>();
    }

    public async Task AddAsync(TEntity entity)
    {
        await this.DbSet.AddAsync(entity);
        await this.Context.SaveChangesAsync();
    }

    public async Task DeleteAsync(TEntity entity)
    {
        this.DbSet.Remove(entity);
        await this.Context.SaveChangesAsync();
    }

    public async Task DeleteByIdAsync(int id)
    {
        var entity = await this.DbSet.FindAsync(id);
        if (entity != null)
        {
            await this.DeleteAsync(entity);
        }
    }

    public async Task<IEnumerable<TEntity>> GetAllAsync()
    {
        return await this.DbSet.ToListAsync();
    }

    // Додаємо знак питання до TEntity
    public async Task<TEntity?> GetByIdAsync(int id)
    {
        return await this.DbSet.FindAsync(id);
    }

    public async Task UpdateAsync(TEntity entity)
    {
        if (this.Context.Entry(entity).State == EntityState.Detached)
        {
            this.DbSet.Attach(entity);
        }
        this.Context.Entry(entity).State = EntityState.Modified;

        await this.Context.SaveChangesAsync();
    }
}
