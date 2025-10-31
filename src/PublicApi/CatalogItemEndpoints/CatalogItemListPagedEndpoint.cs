using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MinimalApi.Endpoint;
using Microsoft.eShopWeb.ApplicationCore.Catalog.Abstractions;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;

namespace Microsoft.eShopWeb.PublicApi.CatalogItemEndpoints;

public class CatalogItemListPagedEndpoint : IEndpoint<IResult, ListPagedCatalogItemRequest, ICatalogFacade>
{
    private readonly IUriComposer _uriComposer;
    private readonly IMapper _mapper;

    public CatalogItemListPagedEndpoint(IUriComposer uriComposer, IMapper mapper)
    {
        _uriComposer = uriComposer;
        _mapper = mapper;
    }

    public void AddRoute(IEndpointRouteBuilder app)
    {
        app.MapGet("api/catalog-items",
            async (
                int? pageSize,
                int? pageIndex,
                int? catalogBrandId,
                int? catalogTypeId,
                ICatalogFacade catalog,
                IUriComposer uriComposer,
                IMapper mapper) =>
            {
                var ep = new CatalogItemListPagedEndpoint(uriComposer, mapper);
                var req = new ListPagedCatalogItemRequest(pageSize, pageIndex, catalogBrandId, catalogTypeId);
                return await ep.HandleAsync(req, catalog);
            })
            .Produces<ListPagedCatalogItemResponse>()
            .WithTags("CatalogItemEndpoints");
    }

    public async Task<IResult> HandleAsync(ListPagedCatalogItemRequest request, ICatalogFacade catalog)
    {
        var response = new ListPagedCatalogItemResponse(request.CorrelationId());

        var page = await catalog.GetProductsAsync(
            brandId: request.CatalogBrandId,
            typeId: request.CatalogTypeId,
            pageIndex: request.PageIndex,
            pageSize: request.PageSize);

        response.CatalogItems = page.Items.Select(p =>
        {
            var dto = _mapper.Map<CatalogItemDto>(p);
            dto.PictureUri = _uriComposer.ComposePicUri(p.PictureUri ?? string.Empty);
            return dto;
        }).ToList();

        if (request.PageSize > 0)
            response.PageCount = (int)Math.Ceiling((decimal)page.TotalCount / request.PageSize);
        else
            response.PageCount = page.TotalCount > 0 ? 1 : 0;

        return Results.Ok(response);
    }
}
