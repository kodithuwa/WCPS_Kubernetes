using Microsoft.EntityFrameworkCore;
using WCPS.DATA;
using WCPS.Repository.Contract;

namespace WCPS.Repository;
public class Repository<T> : IRepository where T : class
{
    private readonly StorageContext _context;
    private readonly DbSet<T> _dbSet;

    public Repository(StorageContext context)
    {
        _context = context;
        _dbSet = _context.Set<T>();
    }

    //public async Task<IEnumerable<T>> GetAllAsync() => await _dbSet.ToListAsync();

    //public async Task<T?> GetByIdAsync(int id) => await _dbSet.FindAsync(id);

    //public async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);

    //public void Update(T entity) => _dbSet.Update(entity);

    //public void Delete(T entity) => _dbSet.Remove(entity);

    //public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    //{
    //    return await _dbSet.Where(predicate).ToListAsync();
    //}

    //public Task UpdateAsync(T entity)
    //{
    //    throw new NotImplementedException();
    //}

    //public Task DeleteAsync(int id)
    //{
    //    throw new NotImplementedException();
    //}
}
