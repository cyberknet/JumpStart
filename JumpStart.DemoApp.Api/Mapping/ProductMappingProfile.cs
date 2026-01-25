using JumpStart.Api.Mapping;
using JumpStart.DemoApp.Data;
using JumpStart.DemoApp.Shared.DTOs;

namespace JumpStart.DemoApp.Mapping;

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

    protected override void ConfigureAdditionalMappings()
    {
        // Example: Custom mapping for a computed property
        // CreateMap<Product, ProductDto>()
        //     .ForMember(dest => dest.SomeComputedProperty, 
        //                opt => opt.MapFrom(src => src.Price * 1.1m));

        // For CreateProductDto to Product, set default IsActive to true
        CreateMap<CreateProductDto, Product>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true));

        // For UpdateProductDto to Product, preserve the SKU (don't map it)
        CreateMap<UpdateProductDto, Product>()
            .ForMember(dest => dest.SKU, opt => opt.Ignore());
    }
}
