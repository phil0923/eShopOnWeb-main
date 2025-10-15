using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MinimalApi.Endpoint;
using Microsoft.eShopWeb.ApplicationCore.Catalog.Abstractions;

namespace Microsoft.eShopWeb.PublicApi.CatalogTypeEndpoints;

/// <summary>
/// List Catalog Types via ICatalogFacade
/// </summary>
public class CatalogTypeListEndpoint : IEndpoint<IResult, ICatalogFacade>
{
    public void AddRoute(IEndpointRouteBuilder app)
    {
        app.MapGet("api/catalog-types",
            async (ICatalogFacade catalog) =>
            {
                return await HandleAsync(catalog);
            })
           .Produces<ListCatalogTypesResponse>()
           .WithTags("CatalogTypeEndpoints");
    }

    public async Task<IResult> HandleAsync(ICatalogFacade catalog)
    {
        var response = new ListCatalogTypesResponse();
        var types = await catalog.GetTypesAsync();

        response.CatalogTypes = types
            .Select(t => new CatalogTypeDto { Id = t.Id, Name = t.Name })
            .ToList();

        return Results.Ok(response);
    }
}
