using JumpStart.Data.Auditing;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JumpStart.DemoApp.Data;

/// <summary>
/// Represents a product in the application.
/// Uses JumpStart's Simple (Guid-based) auditable entity with automatic tracking.
/// </summary>
[Index(nameof(SKU), IsUnique = true)]
public class Product : SimpleAuditableNamedEntity
{
    /// <summary>
    /// Gets or sets the product description.
    /// </summary>
    [StringLength(200)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the product price.
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    /// <summary>
    /// Gets or sets the current stock quantity.
    /// </summary>
    public int StockQuantity { get; set; }

    /// <summary>
    /// Gets or sets the product SKU (Stock Keeping Unit).
    /// </summary>
    [Required]
    [StringLength(50)]
    public string SKU { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether the product is currently active/available.
    /// </summary>
    public bool IsActive { get; set; } = true;
}