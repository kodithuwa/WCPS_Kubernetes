using WCPS.Service.Model;

namespace WCPS.Repository.Contract;

public interface IProductRepository : IRepository
{
    IQueryable<Product> Get();

    Task<Product?> GetByIdAsync(int id);

    Task<Product?> CreateAsync(Product product);
}