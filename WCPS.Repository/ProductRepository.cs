using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using WCPS.Data.Model;
using WCPS.DATA;
using WCPS.Repository.Contract;
using BO = WCPS.Service.Model;

namespace WCPS.Repository;
public class ProductRepository : Repository<Product>, IProductRepository
{
    private readonly StorageContext _context;
    private readonly DbSet<Product> _dbSet;
    private IMapper _mapper;

    public ProductRepository(StorageContext context, IMapper mapper) : base(context)
    {
        _context = context;
        _dbSet = _context.Set<Product>();
        _mapper = mapper;
    }

    public IQueryable<BO.Product> Get()
    {
        var query = _dbSet.AsQueryable();
        var productList = query.ProjectTo<BO.Product>(_mapper.ConfigurationProvider);
        return productList;
    }

    public async Task<BO.Product?> GetByIdAsync(int id)
    {
        var product = await _dbSet.FindAsync(id);
        return product == null ? null : _mapper.Map<BO.Product>(product);
    }

    public async Task<BO.Product?> CreateAsync(BO.Product product)
    {
        var entity = _mapper.Map<Product>(product);
        var result = await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
        return _mapper.Map<BO.Product>(result.Entity);
    }
}
