using AutoMapper;
using Microsoft.eShopWeb.ApplicationCore.Catalog.Abstractions;
using Microsoft.eShopWeb.PublicApi.CatalogItemEndpoints;
using Microsoft.eShopWeb.PublicApi.CatalogBrandEndpoints;
using Microsoft.eShopWeb.PublicApi.CatalogTypeEndpoints;

namespace Microsoft.eShopWeb.PublicApi;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<ProductDTO, CatalogItemDto>()
            .ForMember(d => d.CatalogBrandId, o => o.MapFrom(s => s.BrandId))
            .ForMember(d => d.CatalogTypeId,  o => o.MapFrom(s => s.TypeId))
            .ForMember(d => d.PictureUri,     o => o.MapFrom(s => s.PictureUri));

        CreateMap<BrandDTO, CatalogBrandDto>()
            .ForMember(d => d.Name, o => o.MapFrom(s => s.Name));

        CreateMap<TypeDTO, CatalogTypeDto>()
            .ForMember(d => d.Name, o => o.MapFrom(s => s.Name));
    }
}
