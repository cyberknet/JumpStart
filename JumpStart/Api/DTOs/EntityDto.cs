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

namespace JumpStart.Api.DTOs;

/// <summary>
/// Base DTO for entities with a Guid identifier.
/// Provides the Id property for read operations and serves as the foundation for all JumpStart DTOs.
/// </summary>
/// <remarks>
/// <para>
/// This is the base class for all DTOs in the JumpStart framework. It implements <see cref="JumpStart.Api.DTOs.IDto"/> and provides the essential Id property required for entity identification.
/// </para>
/// <para>
/// Derived classes should add additional properties specific to their entity type. For entities requiring audit information, inherit from <see cref="JumpStart.Api.DTOs.AuditableEntityDto"/> instead.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Example: Creating a form DTO
/// public class FormDto : JumpStart.Api.DTOs.EntityDto
/// {
///     public string Name { get; set; } = string.Empty;
///     public string Description { get; set; } = string.Empty;
/// }
/// 
/// // Usage in API response:
/// // GET /api/forms/6bcc90de-5214-4d5f-8b6b-68d443551A9c
/// // {
/// //   "id": "6bcc90de-5214-4d5f-8b6b-68d443551A9c",
/// //   "name": "Form Name",
/// //   "description": "Survey for feedback"
/// // }
/// </code>
/// </example>
/// <seealso cref="JumpStart.Api.DTOs.IDto"/>
/// <seealso cref="JumpStart.Api.DTOs.AuditableEntityDto"/>
public abstract class EntityDto : IDto
{
    /// <summary>
    /// Gets or sets the unique identifier for the entity.
    /// </summary>
    /// <value>
    /// A value of type <typeref name="Guid"/> that uniquely identifies this entity.
    /// This corresponds to the primary key of the underlying entity in the data store.
    /// </value>
    /// <remarks>
    /// <para>
    /// The Id is populated when reading entities from the data store and should be included
    /// in update operations to identify which entity to modify. It is typically excluded from
    /// create DTOs as the ID is usually assigned by the database or repository.
    /// </para>
    /// </remarks>
    public Guid Id { get; set; }
}
