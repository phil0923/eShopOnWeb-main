using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.eShopWeb.ApplicationCore.Catalog.Abstractions
{
    public sealed record ProductDTO(
        int Id,
        string Name,
        string? Description,
        decimal Price,
        string PictureUri,
        int BrandId,
        int TypeId
    );

    public sealed record BrandDTO(int Id, string Name);

    public sealed record TypeDTO(int Id, string Name);

    public sealed record PagedResult<T>(
        IReadOnlyList<T> Items,
        int PageIndex,
        int PageSize,
        int TotalCount
    );
    public interface ICatalogFacade
    {
        Task<PagedResult<ProductDTO>> GetProductsAsync(
            string? search, int? brandId, int? typeId,
            int pageIndex, int pageSize,
            CancellationToken ct = default);

        Task<ProductDTO?> GetProductByIdAsync(int id, CancellationToken ct = default);

        Task<IReadOnlyList<BrandDTO>> GetBrandsAsync(CancellationToken ct = default);

        Task<IReadOnlyList<TypeDTO>> GetTypesAsync(CancellationToken ct = default);
    }
}
