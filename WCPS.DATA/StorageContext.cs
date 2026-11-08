using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WCPS.Data.Model;

namespace WCPS.DATA;

public class StorageContext : IdentityDbContext<IdentityUser>
{
    public StorageContext(DbContextOptions<StorageContext> options)
       : base(options) { }

    public DbSet<Product> Products => Set<Product>();

}

