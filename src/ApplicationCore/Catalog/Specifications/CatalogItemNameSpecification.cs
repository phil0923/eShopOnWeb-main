using Ardalis.Specification;
using Microsoft.eShopWeb.ApplicationCore.Catalog.Entities;

namespace Microsoft.eShopWeb.ApplicationCore.Catalog.Specifications;

public class CatalogItemNameSpecification : Specification<CatalogItem>
{
    public CatalogItemNameSpecification(string catalogItemName)
    {
        Query.Where(item => catalogItemName == item.Name);
    }
}
