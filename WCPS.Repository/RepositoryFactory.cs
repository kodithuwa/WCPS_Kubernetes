using WCPS.Repository.Contract;

namespace WCPS.Repository;
public class RepositoryFactory : IRepositoryFactory
{
    private readonly IServiceProvider _serviceProvider;

    public RepositoryFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IRepository GetRepository<T>() where T : IRepository
    {
        // Try to find a custom repository interface like IProductRepository for Product
        var customRepoInterfaceName = typeof(T).Name;

        // Search for the interface in the same assembly as IRepository<T>
        var contractAssembly = typeof(IRepository).Assembly;
        var customRepoInterface = contractAssembly
            .GetTypes()
            .FirstOrDefault(t => t.IsInterface && t.Name == customRepoInterfaceName);

        if (customRepoInterface == null)
        {
            throw new InvalidOperationException(
                $"Custom repository interface not found for type: {typeof(T).Name}");
        }

        var customRepo = _serviceProvider.GetService(customRepoInterface);
        return (IRepository)customRepo;
    }
}
