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

namespace JumpStart.Data;

/// <summary>
/// Defines the base contract for all user entities in the system.
/// User entities represent authenticated users who can perform auditable actions.
/// This marker interface distinguishes user entities from other entities for audit tracking and user context operations.
/// </summary>
/// <remarks>
/// <para>
/// This interface extends <see cref="IEntity"/> to provide type-safe identification of user entities
/// throughout the JumpStart framework. It serves as a marker interface with no additional members,
/// allowing the framework to distinguish user entities from other entities in generic code, particularly
/// for audit tracking where user information is recorded.
/// </para>
/// <para>
/// <strong>Purpose and Design:</strong>
/// The primary purpose of this interface is to enable type-safe user identification in audit tracking systems.
/// When audit fields like CreatedById, ModifiedById, or DeletedById are populated, they reference entities
/// that implement IUser. This provides compile-time safety and enables navigation properties to user entities.
/// </para>
/// <para>
/// <strong>When to Use:</strong>
/// Implement this interface (or inherit from a base class that implements it) when:
/// - Creating user/account entities that can perform auditable actions
/// - Building authentication and authorization systems
/// - Tracking who performed specific operations (audit trails)
/// </para>
/// <para>
/// <strong>Alternative Options:</strong>
/// - Custom user base classes that implement IUser for specific requirements
/// </para>
/// <para>
/// <strong>Audit Tracking Integration:</strong>
/// This interface is fundamental to the audit tracking system. 
/// - CreatedById references a user implementing IUser
/// - ModifiedById references a user implementing IUser
/// - DeletedById references a user implementing IUser
/// </para>
/// <para>
/// <strong>Typical Implementation Pattern:</strong>
/// User entities typically include additional properties such as Username, Email, PasswordHash,
/// FirstName, LastName, IsActive, Roles, etc. The interface itself is minimal by design,
/// allowing flexibility in how user entities are structured while maintaining type safety for identification.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Example: Simple user entity implementing IUser
/// public class User : JumpStart.Data.Entity, JumpStart.Data.IUser
/// {
///     [System.ComponentModel.DataAnnotations.Required]
///     [System.ComponentModel.DataAnnotations.StringLength(50)]
///     public string Username { get; set; } = string.Empty;
///     [System.ComponentModel.DataAnnotations.Required]
///     [System.ComponentModel.DataAnnotations.EmailAddress]
///     public string Email { get; set; } = string.Empty;
///     public string PasswordHash { get; set; } = string.Empty;
///     public bool IsActive { get; set; } = true;
///     public System.DateTime CreatedDate { get; set; }
/// }
/// </code>
/// </example>
/// <seealso cref="IEntity"/>
/// <seealso cref="Auditing.ICreatable"/>
/// <seealso cref="Auditing.IModifiable"/>
/// <seealso cref="Auditing.IDeletable"/>
public interface IUser : IEntity
{
    // This interface intentionally contains no members beyond those inherited from IEntity
    // It serves as a marker interface to distinguish user entities from other entities,
    // enabling type-safe user identification in audit tracking and user context operations.
}
