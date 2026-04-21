using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using FixIt.DAL.Data;

namespace FixIt.DAL.Repositories;

public interface IGenericRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<T?> GetByIdAsync(string id);
    IQueryable<T> GetAll();
    IQueryable<T> GetAll(params Expression<Func<T, object>>[] includes);
    Task AddAsync(T entity);
    void Update(T entity);
    void Delete(T entity);
}

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly FixItDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public GenericRepository(FixItDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(int id)
        => await _dbSet.FindAsync(id);

    public async Task<T?> GetByIdAsync(string id)
        => await _dbSet.FindAsync(id);

    public IQueryable<T> GetAll()
        => _dbSet.AsNoTracking();

    public IQueryable<T> GetAll(params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = _dbSet.AsNoTracking();
        foreach (var include in includes)
            query = query.Include(include);
        return query;
    }

    public async Task AddAsync(T entity)
        => await _dbSet.AddAsync(entity);

    public void Update(T entity)
        => _dbSet.Update(entity);

    public void Delete(T entity)
        => _dbSet.Remove(entity);
}
