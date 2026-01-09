using JumpStart.Data.Auditing;

namespace JumpStart.DemoApp.Data;

/// <summary>
/// Represents a product in the application.
/// Uses JumpStart's Simple (Guid-based) auditable entity with automatic tracking.
/// </summary>
public class Product : SimpleAuditableNamedEntity
{
    /// <summary>
    /// Gets or sets the product description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the product price.
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Gets or sets the current stock quantity.
    /// </summary>
    public int StockQuantity { get; set; }

    /// <summary>
    /// Gets or sets the product SKU (Stock Keeping Unit).
    /// </summary>
    public string SKU { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether the product is currently active/available.
    /// </summary>
    public bool IsActive { get; set; } = true;
}