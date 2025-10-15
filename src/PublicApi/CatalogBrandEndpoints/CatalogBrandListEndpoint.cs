using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MinimalApi.Endpoint;
using Microsoft.eShopWeb.ApplicationCore.Catalog.Abstractions;

namespace Microsoft.eShopWeb.PublicApi.CatalogBrandEndpoints;

/// <summary>
/// List Catalog Brands via ICatalogFacade
/// </summary>
public class CatalogBrandListEndpoint : IEndpoint<IResult, ICatalogFacade>
{
    public void AddRoute(IEndpointRouteBuilder app)
    {
        app.MapGet("api/catalog-brands",
            async (ICatalogFacade catalog) =>
            {
                return await HandleAsync(catalog);
            })
           .Produces<ListCatalogBrandsResponse>()
           .WithTags("CatalogBrandEndpoints");
    }

    // NB: MinimalApi.Endpoint forventer netop denne signatur
    public async Task<IResult> HandleAsync(ICatalogFacade catalog)
    {
        var response = new ListCatalogBrandsResponse();
        var brands = await catalog.GetBrandsAsync();

        response.CatalogBrands = brands
            .Select(b => new CatalogBrandDto { Id = b.Id, Name = b.Name })
            .ToList();

        return Results.Ok(response);
    }
}
