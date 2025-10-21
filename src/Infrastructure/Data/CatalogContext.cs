using Microsoft.EntityFrameworkCore;
using Microsoft.eShopWeb.ApplicationCore.Catalog.Entities;
using Microsoft.eShopWeb.Infrastructure.Data.Config;

namespace Microsoft.eShopWeb.Infrastructure.Data
{
    public class CatalogContext : DbContext
    {
        public CatalogContext(DbContextOptions<CatalogContext> options) : base(options) { }

        public DbSet<CatalogItem> CatalogItems => Set<CatalogItem>();
        public DbSet<CatalogBrand> CatalogBrands => Set<CatalogBrand>();
        public DbSet<CatalogType> CatalogTypes => Set<CatalogType>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new CatalogItemConfiguration());
            modelBuilder.ApplyConfiguration(new CatalogBrandConfiguration());
            modelBuilder.ApplyConfiguration(new CatalogTypeConfiguration());
        }
    }
}
