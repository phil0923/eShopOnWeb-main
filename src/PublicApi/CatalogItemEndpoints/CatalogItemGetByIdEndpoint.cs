using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MinimalApi.Endpoint;
using Microsoft.eShopWeb.ApplicationCore.Catalog.Abstractions;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;

namespace Microsoft.eShopWeb.PublicApi.CatalogItemEndpoints;

/// <summary>
/// Get a Catalog Item by Id via ICatalogFacade
/// </summary>
public class CatalogItemGetByIdEndpoint : IEndpoint<IResult, GetByIdCatalogItemRequest, ICatalogFacade>
{
    private readonly IUriComposer _uriComposer;

    public CatalogItemGetByIdEndpoint(IUriComposer uriComposer)
    {
        _uriComposer = uriComposer;
    }

    public void AddRoute(IEndpointRouteBuilder app)
    {
        app.MapGet("api/catalog-items/{catalogItemId:int}",
            async (int catalogItemId, ICatalogFacade catalog, IUriComposer uriComposer, CancellationToken ct) =>
            {
                var ep = new CatalogItemGetByIdEndpoint(uriComposer);
                return await ep.HandleAsync(new GetByIdCatalogItemRequest(catalogItemId), catalog);
            })
           .Produces<GetByIdCatalogItemResponse>()
           .WithTags("CatalogItemEndpoints");
    }

    // NB: MinimalApi.Endpoint forventer netop denne signatur
    public async Task<IResult> HandleAsync(GetByIdCatalogItemRequest request, ICatalogFacade catalog)
    {
        var response = new GetByIdCatalogItemResponse(request.CorrelationId());

        var p = await catalog.GetProductByIdAsync(request.CatalogItemId);
        if (p is null) return Results.NotFound();

        response.CatalogItem = new CatalogItemDto
        {
            Id = p.Id,
            CatalogBrandId = p.BrandId,
            CatalogTypeId = p.TypeId,
            Description = p.Description,
            Name = p.Name,
            PictureUri = _uriComposer.ComposePicUri(p.PictureUri ?? string.Empty),
            Price = p.Price
        };

        return Results.Ok(response);
    }
}
