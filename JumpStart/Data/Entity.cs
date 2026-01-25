// Copyright ©2026 Scott Blomfield
/*
 *  This program is free software: you can redistribute it and/or modify it under the terms of the
 *  GNU General Public License as published by the Free Software Foundation, either version 3 of the
 *  License, or (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without
 *  even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
 *  General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License along with this program. If not,
 *  see <https://www.gnu.org/licenses/>. 
 */

using System;
using System.ComponentModel.DataAnnotations;

namespace JumpStart.Data;

/// <summary>
/// Provides a base implementation of the <see cref="JumpStart.Data.IEntity"/> interface for entities with a unique <see cref="System.Guid"/> identifier.
/// This abstract class serves as the foundation for all entities in the JumpStart framework.
/// </summary>
/// <remarks>
/// <para>
/// This class provides the core identity functionality for entities in the JumpStart framework. It implements
/// the <see cref="JumpStart.Data.IEntity"/> interface and provides a strongly-typed <c>Id</c> property decorated with the
/// <see cref="System.ComponentModel.DataAnnotations.KeyAttribute"/> for automatic recognition by Entity Framework Core and other ORMs.
/// </para>
/// <para>
/// <strong>Alternative Base Classes:</strong>
/// For extended scenarios, consider these alternatives:
/// - <see cref="Data.Auditing.AuditableEntity"/> - Adds full audit tracking (creation, modification, deletion)
/// - <see cref="JumpStart.Data.NamedEntity"/> - Adds Name property for entities that need human-readable names
/// </para>
/// <para>
/// <strong>Entity Framework Core Integration:</strong>
/// The <see cref="KeyAttribute"/> on the <c>Id</c> property ensures automatic recognition as the primary key.
/// EF Core will automatically configure the appropriate database column type and constraints for <c>Guid</c>.
/// </para>
/// <para>
/// <strong>Inheritance Hierarchy:</strong>
/// Entity is the base for several specialized entity types in the framework:
/// - AuditableEntity extends this with audit fields
/// - NamedEntity extends this with a Name property
/// - AuditableNamedEntity combines both naming and auditing
/// All inherit the strongly-typed <c>Id</c> property and <c><see cref="JumpStart.Data.IEntity"/></c> implementation.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Example: Simple entity with Guid identifier
/// public class Product : JumpStart.Data.Entity
/// {
///     [System.ComponentModel.DataAnnotations.Required]
///     [System.ComponentModel.DataAnnotations.StringLength(200)]
///     public string Name { get; set; } = string.Empty;
///     [System.ComponentModel.DataAnnotations.Range(0.01, double.MaxValue)]
///     public decimal Price { get; set; }
///     public string? Description { get; set; }
/// }
/// </code>
/// </example>
/// <seealso cref="JumpStart.Data.IEntity"/>
/// <seealso cref="Data.Auditing.AuditableEntity"/>
/// <seealso cref="JumpStart.Data.NamedEntity"/>
public abstract class Entity : IEntity
{
    /// <summary>
    /// Gets or sets the unique identifier for the entity.
    /// This property is marked with the <see cref="KeyAttribute"/> to indicate it is the primary key.
    /// </summary>
    /// <value>
    /// The unique identifier of type <typeref name="Guid"/>.
    /// </value>
    /// <remarks>
    /// <para>
    /// The <see cref="KeyAttribute"/> decoration ensures this property is recognized as the primary key
    /// by Entity Framework Core and other ORMs without requiring additional configuration in most cases.
    /// </para>
    /// <para>
    /// <strong>Key Generation Strategies:</strong>
    /// Can be application-generated (Guid.NewGuid()) or database-generated (NEWSEQUENTIALID())
    /// </para>
    /// <para>
    /// <strong>Usage in Repositories:</strong>
    /// Use the Id property to determine if an entity is new (Id == default) or existing (Id != default).
    /// This is particularly important for insert vs update logic in generic repositories.
    /// </para>
    /// </remarks>
    [Key]
    public Guid Id { get; set; }
}
