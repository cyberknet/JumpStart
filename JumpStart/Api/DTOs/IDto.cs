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
/// Marker interface for all Data Transfer Objects (DTOs) in the JumpStart framework.
/// Provides a common type for all DTOs to enable generic handling and type identification.
/// </summary>
/// <remarks>
/// <para>
/// This interface serves as the root marker interface for all DTOs in the JumpStart framework.
/// It follows the marker interface pattern, containing no methods or properties, and is used
/// purely for type identification and polymorphic handling.
/// </para>
/// <para>
/// <strong>Purpose and Benefits:</strong>
/// - Enables generic constraints for methods and classes that work with any DTO type
/// - Provides compile-time type safety for DTO operations
/// - Supports polymorphic collections of different DTO types
/// - Facilitates framework-level DTO processing (validation, mapping, serialization)
/// - Creates a clear separation between DTOs and other data structures
/// </para>
/// <para>
/// <strong>DTO Hierarchy:</strong>
/// All JumpStart DTOs implement this interface either directly or through derived interfaces:
/// - <see cref="ICreateDto"/> - For create (POST) operations (no Id, no audit fields)
/// - <see cref="IUpdateDto{TKey}"/> - For update (PUT) operations (includes Id)
/// - <see cref="JumpStart.Api.DTOs.Advanced.EntityDto{TKey}"/> - For read operations (includes Id)
/// - <see cref="SimpleEntityDto"/> - For read operations with Guid identifiers
/// - <see cref="Advanced.AuditableEntityDto{TKey}"/> - For entities with audit tracking
/// </para>
/// <para>
/// <strong>Design Pattern:</strong>
/// This interface uses the Marker Interface pattern, which is appropriate when:
/// - Type identification is needed without adding behavior
/// - Compile-time type safety is required
/// - Framework-level processing needs to distinguish DTOs from other types
/// - Generic constraints need to work with all DTO types
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Example 1: Using IDto as a generic constraint
/// public class DtoValidator&lt;T&gt; where T : IDto
/// {
///     public ValidationResult Validate(T dto)
///     {
///         // Can validate any DTO type
///         return ValidationResult.Success;
///     }
/// }
/// 
/// // Example 2: Polymorphic collection of DTOs
/// public class DtoProcessor
/// {
///     public void ProcessDtos(IEnumerable&lt;IDto&gt; dtos)
///     {
///         foreach (var dto in dtos)
///         {
///             // Process different DTO types uniformly
///             Console.WriteLine($"Processing {dto.GetType().Name}");
///         }
///     }
/// }
/// 
/// // Example 3: Type identification
/// public bool IsDto(object obj)
/// {
///     return obj is IDto;
/// }
/// 
/// // Example 4: Creating a concrete DTO
/// public class ProductDto : EntityDto&lt;int&gt;
/// {
///     public string Name { get; set; } = string.Empty;
///     public decimal Price { get; set; }
/// }
/// // ProductDto implements IDto through EntityDto&lt;int&gt;
/// 
/// // Example 5: Framework-level usage
/// public class ApiResponseBuilder
/// {
///     public ApiResponse&lt;T&gt; BuildResponse&lt;T&gt;(T data) where T : IDto
///     {
///         return new ApiResponse&lt;T&gt;
///         {
///             Data = data,
///             Success = true,
///             Timestamp = DateTime.UtcNow
///         };
///     }
/// }
/// </code>
/// </example>
/// <seealso cref="ICreateDto"/>
/// <seealso cref="IUpdateDto{TKey}"/>
/// <seealso cref="JumpStart.Api.DTOs.Advanced.EntityDto{TKey}"/>
/// <seealso cref="SimpleEntityDto"/>
public interface IDto
{
}
