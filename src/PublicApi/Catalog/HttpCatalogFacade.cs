using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using Microsoft.eShopWeb.ApplicationCore.Catalog.Abstractions;

namespace Microsoft.eShopWeb.PublicApi.Catalog
{
    public sealed class HttpCatalogFacade : ICatalogFacade
    {
        private readonly HttpClient _http;
        private readonly string _baseApi;

        public class CatalogServiceOptions
        {
            public string BaseUrl { get; set; } = "";
        }

        public HttpCatalogFacade(HttpClient http, IOptions<CatalogServiceOptions> opt)
        {
            _http = http;
            _baseApi = opt.Value.BaseUrl.TrimEnd('/');
        }

        public async Task<PagedResult<ProductDTO>> GetProductsAsync(
            int? brandId, int? typeId,
            int pageIndex, int pageSize,
            CancellationToken ct = default)
        {
            var query = new List<string> { $"pageIndex={pageIndex}", $"pageSize={pageSize}" };
            if (brandId.HasValue) query.Add($"catalogBrandId={brandId.Value}");
            if (typeId.HasValue) query.Add($"catalogTypeId={typeId.Value}");

            var url = $"{_baseApi}/api/catalog/items?{string.Join('&', query)}";

            var resp = await _http.GetFromJsonAsync<PagedItemsResponse>(url, ct)
                       ?? new PagedItemsResponse();

            var items = resp.Items.Select(i => new ProductDTO(
                i.Id,
                i.Name ?? "",
                i.Description,
                i.Price,
                i.PictureUri ?? "",
                i.CatalogBrandId,
                i.CatalogTypeId
            )).ToList();

            return new PagedResult<ProductDTO>(items, resp.PageIndex, resp.PageSize, resp.TotalItems);
        }

        public async Task<ProductDTO?> GetProductByIdAsync(int id, CancellationToken ct = default)
        {
            var url = $"{_baseApi}/api/catalog/items/{id}";
            var i = await _http.GetFromJsonAsync<CatalogItemDto>(url, ct);
            return i is null ? null : new ProductDTO(
                i.Id,
                i.Name ?? "",
                i.Description,
                i.Price,
                i.PictureUri ?? "",
                i.CatalogBrandId,
                i.CatalogTypeId
            );
        }

        public async Task<IReadOnlyList<BrandDTO>> GetBrandsAsync(CancellationToken ct = default)
        {
            var url = $"{_baseApi}/api/catalog/brands";
            var list = await _http.GetFromJsonAsync<List<CatalogBrandDto>>(url, ct)
                       ?? new List<CatalogBrandDto>();
            return list.Select(b => new BrandDTO(b.Id, b.Brand ?? "")).ToList();
        }

        public async Task<IReadOnlyList<TypeDTO>> GetTypesAsync(CancellationToken ct = default)
        {
            var url = $"{_baseApi}/api/catalog/types";
            var list = await _http.GetFromJsonAsync<List<CatalogTypeDto>>(url, ct)
                       ?? new List<CatalogTypeDto>();
            return list.Select(t => new TypeDTO(t.Id, t.Type ?? "")).ToList();
        }

        private sealed class PagedItemsResponse
        {
            public List<CatalogItemDto> Items { get; set; } = new();
            public int PageIndex { get; set; }
            public int PageSize { get; set; }
            public int TotalItems { get; set; }
        }

        private sealed class CatalogItemDto
        {
            public int Id { get; set; }
            public string? Name { get; set; }
            public string? Description { get; set; }
            public decimal Price { get; set; }
            public string? PictureUri { get; set; }
            public int CatalogBrandId { get; set; }
            public int CatalogTypeId { get; set; }
        }

        private sealed class CatalogBrandDto
        {
            public int Id { get; set; }
            public string? Brand { get; set; }
        }

        private sealed class CatalogTypeDto
        {
            public int Id { get; set; }
            public string? Type { get; set; }
        }
    }
}
