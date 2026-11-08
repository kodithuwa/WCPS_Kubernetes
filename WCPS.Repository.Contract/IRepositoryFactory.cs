using WCPS.Repository.Contract;

public interface IRepositoryFactory
{
    IRepository GetRepository<T>() where T : IRepository;
}
