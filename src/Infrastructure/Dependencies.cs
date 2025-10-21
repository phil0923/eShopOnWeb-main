using Microsoft.EntityFrameworkCore;
using Microsoft.eShopWeb.Infrastructure.Data;
using Microsoft.eShopWeb.Infrastructure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.eShopWeb.Infrastructure
{
    public static class Dependencies
    {
        public static IServiceCollection AddCatalogDb(this IServiceCollection services, IConfiguration configuration)
        {
            var useOnlyInMemoryDatabase = configuration.GetValue<bool>("UseOnlyInMemoryDatabase");

            if (useOnlyInMemoryDatabase)
            {
                services.AddDbContext<CatalogContext>(c => c.UseInMemoryDatabase("Catalog"));
            }
            else
            {
                services.AddDbContext<CatalogContext>(c =>
                    c.UseSqlServer(configuration.GetConnectionString("CatalogConnection")));
            }

            return services;
        }

        public static IServiceCollection AddIdentityDb(this IServiceCollection services, IConfiguration configuration)
        {
            var useOnlyInMemoryDatabase = configuration.GetValue<bool>("UseOnlyInMemoryDatabase");

            if (useOnlyInMemoryDatabase)
            {
                services.AddDbContext<AppIdentityDbContext>(options =>
                    options.UseInMemoryDatabase("Identity"));
            }
            else
            {
                services.AddDbContext<AppIdentityDbContext>(options =>
                    options.UseSqlServer(configuration.GetConnectionString("IdentityConnection")));
            }

            return services;
        }
    }
}
