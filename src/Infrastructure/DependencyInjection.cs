using Microsoft.eShopWeb.ApplicationCore.Catalog.Abstractions;
using Infrastructure.Catalog;
using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependenciesInjection
{
    public static IServiceCollection AddCatalogProvider(this IServiceCollection services, IConfiguration config)
    {
        var provider = config.GetSection("Catalog:Provider").Value ?? "Ef";
        if (provider.Equals("Ef", StringComparison.OrdinalIgnoreCase))
        {
            services.AddScoped<ICatalogFacade, CatalogFacadeEf>();
        }
        else
        {
            throw new InvalidOperationException($"Catalog provider '{provider}' is not recognized.");
        }

        return services;
    }
}