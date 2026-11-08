namespace WCPS.Repository.Contract;

public interface IUnitOfWork : IDisposable
{
    IRepository Repository<T>() where T : IRepository;
    Task<int> SaveChangesAsync();
}
