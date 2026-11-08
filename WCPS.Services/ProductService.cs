using WCPS.Repository.Contract;
using WCPS.Service.Model;

namespace WCPS.Service.contract;

public class ProductService : IProductService
{
    private readonly IUnitOfWork _uow;
    public ProductService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Product> CreateAsync(Product product)
    {
       var _repository = (IProductRepository) _uow.Repository<IProductRepository>();
       var result = await _repository.CreateAsync(product);
        return result ?? throw new InvalidOperationException("Failed to create product.");
    }

    public IQueryable<Product> Get()
    {
        var _repository = (IProductRepository)_uow.Repository<IProductRepository>() ;
        return _repository.Get();
    }

}
