using WCPS.DATA;
using WCPS.Repository.Contract;

namespace WCPS.Repository;
public class UnitOfWork : IUnitOfWork
{
    private readonly StorageContext _context;
    private readonly IRepositoryFactory _repositoryFactory;

    public UnitOfWork(StorageContext context, IRepositoryFactory repositoryFactory)
    {
        _context = context;
        _repositoryFactory = repositoryFactory;
    }

    public IRepository Repository<T>() where T : IRepository
    {
        return _repositoryFactory.GetRepository<T>();
    }

   
    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }

}
