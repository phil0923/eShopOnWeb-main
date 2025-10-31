using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

using Microsoft.eShopWeb.ApplicationCore.Catalog.Abstractions;
using Microsoft.eShopWeb.ApplicationCore.Catalog.Entities;
using Microsoft.eShopWeb.Infrastructure.Data;

namespace Infrastructure.Catalog
{
    public sealed class CatalogFacadeEf : ICatalogFacade
    {
        private readonly CatalogContext _db;

        public CatalogFacadeEf(CatalogContext db)
        {
            _db = db;
        }

        public async Task<PagedResult<ProductDTO>> GetProductsAsync(
            int? brandId, int? typeId,
            int pageIndex, int pageSize,
            CancellationToken ct = default)
        {
            if (pageIndex < 0) pageIndex = 0;
            if (pageSize <= 0) pageSize = 10;

            var query = _db.CatalogItems.AsNoTracking().AsQueryable();

            if (brandId is { } b) query = query.Where(p => p.CatalogBrandId == b);
            if (typeId  is { } t) query = query.Where(p => p.CatalogTypeId  == t);

            var total = await query.CountAsync(ct);

            var items = await query
                .OrderBy(p => p.Name)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .Select(p => new ProductDTO(
                    p.Id,
                    p.Name,
                    p.Description,
                    p.Price,
                    p.PictureUri,
                    p.CatalogBrandId,
                    p.CatalogTypeId
                ))
                .ToListAsync(ct);

            return new PagedResult<ProductDTO>(items, pageIndex, pageSize, total);
        }

        public async Task<ProductDTO?> GetProductByIdAsync(int id, CancellationToken ct = default)
        {
            return await _db.CatalogItems
                .AsNoTracking()
                .Where(p => p.Id == id)
                .Select(p => new ProductDTO(
                    p.Id,
                    p.Name,
                    p.Description,
                    p.Price,
                    p.PictureUri,
                    p.CatalogBrandId,
                    p.CatalogTypeId
                ))
                .FirstOrDefaultAsync(ct);
        }

        public async Task<IReadOnlyList<BrandDTO>> GetBrandsAsync(CancellationToken ct = default)
        {
            return await _db.CatalogBrands
                .AsNoTracking()
                .OrderBy(b => b.Brand)
                .Select(b => new BrandDTO(b.Id, b.Brand))
                .ToListAsync(ct);
        }

        public async Task<IReadOnlyList<TypeDTO>> GetTypesAsync(CancellationToken ct = default)
        {
            return await _db.CatalogTypes
                .AsNoTracking()
                .OrderBy(t => t.Type)
                .Select(t => new TypeDTO(t.Id, t.Type))
                .ToListAsync(ct);
        }
    }
}
