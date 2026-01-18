using System;

namespace JumpStart.DemoApp.Shared.DTOs;

/// <summary>
/// DTO for reading Product data.
/// Includes all product information including audit fields.
/// </summary>
public class ProductDto : JumpStart.Api.DTOs.SimpleAuditableEntityDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public string SKU { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

/// <summary>
/// DTO for creating a new Product.
/// Only includes user-provided fields, no Id or audit fields.
/// </summary>
public class CreateProductDto : JumpStart.Api.DTOs.ICreateDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public string SKU { get; set; } = string.Empty;
}

/// <summary>
/// DTO for updating an existing Product.
/// Includes Id and user-modifiable fields, but no audit fields.
/// </summary>
public class UpdateProductDto : JumpStart.Api.DTOs.IUpdateDto<Guid>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public bool IsActive { get; set; }
    // Note: SKU is intentionally omitted - typically shouldn't be changed after creation
}
