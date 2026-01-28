using AutoMapper;
using JumpStart.Api.DTOs;
using JumpStart.Api.Mapping;
using JumpStart.DemoApp.Data;
using JumpStart.DemoApp.Shared.DTOs;

namespace JumpStart.DemoApp.Api.Mapping;

/// <summary>
/// AutoMapper profile for Product entity mappings.
/// Inherits base configuration from SimpleEntityMappingProfile.
/// </summary>
public class ProductMappingProfile : EntityMappingProfile<Product, ProductDto, CreateProductDto, UpdateProductDto>
{
    public ProductMappingProfile()
    {
        // Base class handles standard mappings automatically
        // Add custom mappings here if needed
    }

    protected override void ConfigureAdditionalMappings(IMappingExpression<Product, ProductDto> entityMap, 
                                                        IMappingExpression<CreateProductDto, Product> createMap, 
                                                        IMappingExpression<UpdateProductDto, Product> updateMap)
    {
        // Example: Custom mapping for a computed property
        // CreateMap<Product, ProductDto>()
        //     .ForMember(dest => dest.SomeComputedProperty, 
        //                opt => opt.MapFrom(src => src.Price * 1.1m));

        // For CreateProductDto to Product, set default IsActive to true
        createMap
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true));

        // For UpdateProductDto to Product, preserve the SKU (don't map it)
        updateMap
            .ForMember(dest => dest.SKU, opt => opt.Ignore());
    }
}
