using System.Linq.Expressions;
using Escale.API.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Escale.API.Data.Repositories;

public class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly EscaleDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(EscaleDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(Guid id) => await _dbSet.FindAsync(id);

    public async Task<List<T>> GetAllAsync() => await _dbSet.ToListAsync();

    public IQueryable<T> Query() => _dbSet.AsQueryable();

    public async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);

    public void Update(T entity) => _dbSet.Update(entity);

    public void Remove(T entity) => _dbSet.Remove(entity);

    public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate) => await _dbSet.AnyAsync(predicate);

    public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
        => predicate == null ? await _dbSet.CountAsync() : await _dbSet.CountAsync(predicate);
}
