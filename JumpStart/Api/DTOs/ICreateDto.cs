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

namespace JumpStart.Api.DTOs;

/// <summary>
/// Marker interface for DTOs used in create (POST) operations.
/// Create DTOs should not include Id or audit fields as these are system-generated.
/// </summary>
/// <remarks>
/// <para>
/// This interface serves as a marker to identify DTOs that are specifically designed for
/// entity creation operations. Create DTOs typically contain only the data that a client
/// should provide when creating a new entity, excluding system-managed fields.
/// </para>
/// <para>
/// <strong>Fields to EXCLUDE from Create DTOs:</strong>
/// - Id (assigned by the database or repository)
/// - CreatedById (set from current user context)
/// - CreatedOn (set to current UTC timestamp)
/// - ModifiedById (not applicable for new entities)
/// - ModifiedOn (not applicable for new entities)
/// - DeletedById (not applicable for new entities)
/// - DeletedOn (not applicable for new entities)
/// </para>
/// <para>
/// <strong>Fields to INCLUDE in Create DTOs:</strong>
/// - All business properties that the client should provide
/// - Required fields for entity creation
/// - Optional fields with appropriate nullable types
/// - Navigation properties as IDs or nested create DTOs
/// </para>
/// <para>
/// This interface inherits from <see cref="JumpStart.Api.DTOs.IDto"/> to ensure all create DTOs are part
/// of the DTO hierarchy and can be identified as data transfer objects.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Example: Simple create DTO for a form
/// public class CreateFormDto : JumpStart.Api.DTOs.ICreateDto
/// {
///     [System.ComponentModel.DataAnnotations.Required]
///     [System.ComponentModel.DataAnnotations.StringLength(200)]
///     public string Name { get; set; } = string.Empty;
///     
///     [System.ComponentModel.DataAnnotations.StringLength(1000)]
///     public string? Description { get; set; }
/// }
/// 
/// // Example: Usage in API controller
/// [Microsoft.AspNetCore.Mvc.HttpPost]
/// public async System.Threading.Tasks.Task&lt;Microsoft.AspNetCore.Mvc.ActionResult&lt;FormDto&gt;&gt; Create([Microsoft.AspNetCore.Mvc.FromBody] CreateFormDto createDto)
/// {
///     // The controller receives only the fields the client should provide
///     // System fields (Id, CreatedOn, etc.) are added by the repository
///     var entity = _mapper.Map&lt;JumpStart.Forms.Form&gt;(createDto);
///     var created = await _repository.AddAsync(entity);
///     var dto = _mapper.Map&lt;JumpStart.Api.DTOs.Forms.FormDto&gt;(created);
///     return CreatedAtAction(nameof(GetById), new { id = created.Id }, dto);
/// }
/// </code>
/// </example>
/// <seealso cref="JumpStart.Api.DTOs.IDto"/>
/// <seealso cref="JumpStart.Api.DTOs.IUpdateDto"/>
public interface ICreateDto : IDto
{
}
