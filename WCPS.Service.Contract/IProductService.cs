
using WCPS.Service.Model;

namespace WCPS.Service;

public interface IProductService
{
    IQueryable<Product> Get();

    Task<Product> CreateAsync(Product product);

}
