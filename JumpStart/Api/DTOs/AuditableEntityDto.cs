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
/// Base DTO for auditable entities.
/// Includes read-only audit information (creation and modification tracking) populated by the system.
/// </summary>
/// <remarks>
/// <para>
/// This DTO extends <see cref="JumpStart.Api.DTOs.EntityDto"/> to include audit trail information for entities that track who created and modified them, and when those actions occurred.
/// </para>
/// <para>
/// Audit fields are populated automatically by the repository layer and should be treated as read-only in DTOs. They are included in read operations but excluded from create and update DTOs to prevent clients from manipulating audit data.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Example: Creating a form DTO with audit information
/// public class FormDto : JumpStart.Api.DTOs.AuditableEntityDto
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
/// //   "description": "Survey for feedback",
/// //   "createdById": "1b2c...",
/// //   "createdOn": "2026-01-15T10:30:00Z",
/// //   "modifiedById": "5d6e...",
/// //   "modifiedOn": "2026-01-20T14:45:00Z"
/// // }
/// </code>
/// </example>
/// <seealso cref="JumpStart.Api.DTOs.EntityDto"/>
public abstract class AuditableEntityDto : EntityDto
{
    /// <summary>
    /// Gets or sets the identifier of the user who created this entity.
    /// </summary>
    /// <value>
    /// The user identifier that created this entity.
    /// This value is set by the system during entity creation and should not be modified by clients.
    /// </value>
    /// <remarks>
    /// This field is automatically populated by the repository when the entity is created.
    /// It corresponds to the CreatedById field in entities implementing <see cref="Data.Auditing.ICreatable{T}"/>.
    /// </remarks>
    public Guid CreatedById { get; set; }

    /// <summary>
    /// Gets or sets the date and time (in UTC) when this entity was created.
    /// </summary>
    /// <value>
    /// A <see cref="DateTimeOffset"/> representing when the entity was created.
    /// This value is set by the system during entity creation and should not be modified by clients.
    /// </value>
    /// <remarks>
    /// This field is automatically populated by the repository when the entity is created.
    /// It corresponds to the CreatedOn field in entities implementing <see cref="Data.Auditing.ICreatable{T}"/>.
    /// The value is stored in UTC to ensure consistency across time zones.
    /// </remarks>
    public DateTimeOffset CreatedOn { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who last modified this entity.
    /// </summary>
    /// <value>
    /// The user identifier that last modified this entity, or null if the entity has never been modified.
    /// This value is set by the system during entity updates and should not be modified by clients.
    /// </value>
    /// <remarks>
    /// This field is automatically populated by the repository when the entity is updated.
    /// It corresponds to the ModifiedById field in entities implementing <see cref="Data.Auditing.IModifiable{T}"/>.
    /// A null value indicates the entity has been created but never modified.
    /// </remarks>
    public Guid? ModifiedById { get; set; }

        /// <summary>
        /// Gets or sets the date and time (in UTC) when this entity was last modified.
        /// </summary>
        /// <value>
        /// A <see cref="DateTimeOffset"/> representing when the entity was last modified, or null if never modified.
        /// This value is set by the system during entity updates and should not be modified by clients.
        /// </value>
        /// <remarks>
        /// This field is automatically populated by the repository when the entity is updated.
        /// It corresponds to the ModifiedOn field in entities implementing <see cref="Data.Auditing.IModifiable{T}"/>.
        /// A null value indicates the entity has been created but never modified.
        /// The value is stored in UTC to ensure consistency across time zones.
        /// </remarks>
        public DateTimeOffset? ModifiedOn { get; set; }
    }
