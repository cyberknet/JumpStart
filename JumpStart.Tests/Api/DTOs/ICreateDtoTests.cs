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
using System.Collections.Generic;
using System.Linq;
using JumpStart.Api.DTOs;
using Xunit;

namespace JumpStart.Tests.Api.DTOs;

/// <summary>
/// Unit tests for the <see cref="ICreateDto"/> interface.
/// Tests interface implementation, marker interface pattern, and proper usage for create operations.
/// </summary>
public class ICreateDtoTests
{
    #region Test DTOs

    /// <summary>
    /// Simple create DTO for testing.
    /// </summary>
    public class CreateTestProductDto : ICreateDto
    {
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? Description { get; set; }
    }

    /// <summary>
    /// Create DTO with nested objects.
    /// </summary>
    public class CreateTestOrderDto : ICreateDto
    {
        public DateTime OrderDate { get; set; }
        public List<CreateTestOrderItemDto> Items { get; set; } = new();
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Nested create DTO.
    /// </summary>
    public class CreateTestOrderItemDto : ICreateDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

    /// <summary>
    /// Create DTO with minimal properties.
    /// </summary>
    public class CreateTestMinimalDto : ICreateDto
    {
        public string Value { get; set; } = string.Empty;
    }

    /// <summary>
    /// Intentionally incorrect DTO with Id (should not have Id in create DTOs).
    /// </summary>
    public class IncorrectCreateDto : ICreateDto
    {
        public int Id { get; set; } // Should not be in create DTO
        public string Name { get; set; } = string.Empty;
    }

    #endregion

    #region Interface Inheritance Tests

    [Fact]
    public void ICreateDto_InheritsFrom_IDto()
    {
        // Arrange
        var interfaceType = typeof(ICreateDto);

        // Act
        var inheritsFromIDto = typeof(IDto).IsAssignableFrom(interfaceType);

        // Assert
        Assert.True(inheritsFromIDto, "ICreateDto should inherit from IDto");
    }

    [Fact]
    public void ICreateDto_IsInterface()
    {
        // Arrange
        var interfaceType = typeof(ICreateDto);

        // Act
        var isInterface = interfaceType.IsInterface;

        // Assert
        Assert.True(isInterface, "ICreateDto should be an interface");
    }

    [Fact]
    public void ICreateDto_IsInCorrectNamespace()
    {
        // Arrange
        var interfaceType = typeof(ICreateDto);

        // Act
        var namespaceName = interfaceType.Namespace;

        // Assert
        Assert.Equal("JumpStart.Api.DTOs", namespaceName);
    }

    #endregion

    #region Marker Interface Pattern Tests

    [Fact]
    public void ICreateDto_HasNoMembers()
    {
        // Arrange
        var interfaceType = typeof(ICreateDto);

        // Act
        var declaredMembers = interfaceType.GetMembers(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly);

        // Assert
        Assert.Empty(declaredMembers); // Marker interface has no members
    }

    [Fact]
    public void ICreateDto_CanBeUsedAs_TypeConstraint()
    {
        // Arrange
        var testDto = new CreateTestProductDto { Name = "Test", Price = 10.00m };

        // Act & Assert - This should compile and work
        Assert.IsAssignableFrom<ICreateDto>(testDto);
    }

    [Fact]
    public void ICreateDto_CanIdentify_CreateDtos()
    {
        // Arrange
        var productDto = new CreateTestProductDto();
        var orderDto = new CreateTestOrderDto();

        // Act
        var isProductCreateDto = productDto is ICreateDto;
        var isOrderCreateDto = orderDto is ICreateDto;

        // Assert
        Assert.True(isProductCreateDto);
        Assert.True(isOrderCreateDto);
    }

    #endregion

    #region Implementation Tests

    [Fact]
    public void CreateDto_Implementation_CanSetProperties()
    {
        // Arrange
        var dto = new CreateTestProductDto();

        // Act
        dto.Name = "Test Product";
        dto.Price = 99.99m;
        dto.Description = "Test Description";

        // Assert
        Assert.Equal("Test Product", dto.Name);
        Assert.Equal(99.99m, dto.Price);
        Assert.Equal("Test Description", dto.Description);
    }

    [Fact]
    public void CreateDto_CanBeAssignedTo_IDto()
    {
        // Arrange
        var createDto = new CreateTestProductDto { Name = "Product", Price = 50.00m };

        // Act
        IDto dto = createDto;

        // Assert
        Assert.NotNull(dto);
        Assert.IsAssignableFrom<IDto>(createDto);
        Assert.IsAssignableFrom<ICreateDto>(createDto);
    }

    [Fact]
    public void CreateDto_CanBeAssignedTo_ICreateDto()
    {
        // Arrange
        var dto = new CreateTestProductDto { Name = "Product", Price = 50.00m };

        // Act
        ICreateDto createDto = dto;

        // Assert
        Assert.NotNull(createDto);
    }

    #endregion

    #region Nested Create DTO Tests

    [Fact]
    public void CreateDto_CanContain_NestedCreateDtos()
    {
        // Arrange
        var orderDto = new CreateTestOrderDto
        {
            OrderDate = DateTime.UtcNow,
            Items = new List<CreateTestOrderItemDto>
            {
                new CreateTestOrderItemDto { ProductId = 1, Quantity = 2 },
                new CreateTestOrderItemDto { ProductId = 2, Quantity = 1 }
            },
            Notes = "Test order"
        };

        // Act
        var itemsAreCreateDtos = orderDto.Items.All(item => item is ICreateDto);

        // Assert
        Assert.True(itemsAreCreateDtos);
        Assert.Equal(2, orderDto.Items.Count);
    }

    [Fact]
    public void NestedCreateDto_IsAlso_ICreateDto()
    {
        // Arrange
        var nestedDto = new CreateTestOrderItemDto { ProductId = 1, Quantity = 5 };

        // Act
        var isCreateDto = nestedDto is ICreateDto;
        var isDto = nestedDto is IDto;

        // Assert
        Assert.True(isCreateDto);
        Assert.True(isDto);
    }

    #endregion

    #region Type Identification Tests

    [Fact]
    public void ICreateDto_CanBeIdentified_AtRuntime()
    {
        // Arrange
        object obj = new CreateTestProductDto { Name = "Test", Price = 10.00m };

        // Act
        var isCreateDto = obj is ICreateDto;

        // Assert
        Assert.True(isCreateDto);
    }

    [Fact]
    public void ICreateDto_CanBeUsedIn_Collections()
    {
        // Arrange
        var createDtos = new List<ICreateDto>
        {
            new CreateTestProductDto { Name = "Product1", Price = 10.00m },
            new CreateTestMinimalDto { Value = "Test" },
            new CreateTestOrderItemDto { ProductId = 1, Quantity = 1 }
        };

        // Act
        var count = createDtos.Count;

        // Assert
        Assert.Equal(3, count);
        Assert.All(createDtos, dto => Assert.IsAssignableFrom<ICreateDto>(dto));
    }

    [Fact]
    public void ICreateDto_CanBeFiltered_FromMixedDtos()
    {
        // Arrange
        var dtos = new List<IDto>
        {
            new CreateTestProductDto { Name = "Product", Price = 10.00m },
            new CreateTestMinimalDto { Value = "Test" }
        };

        // Act
        var createDtos = dtos.OfType<ICreateDto>().ToList();

        // Assert
        Assert.Equal(2, createDtos.Count);
        Assert.All(createDtos, dto => Assert.IsAssignableFrom<ICreateDto>(dto));
    }

    #endregion

    #region Property Validation Tests

    [Fact]
    public void CreateDto_ShouldNot_HaveIdProperty()
    {
        // Arrange
        var dtoType = typeof(CreateTestProductDto);

        // Act
        var hasIdProperty = dtoType.GetProperty("Id") != null;

        // Assert
        Assert.False(hasIdProperty, "Create DTOs should not have Id property");
    }

    [Fact]
    public void CreateDto_ShouldNot_HaveAuditProperties()
    {
        // Arrange
        var dtoType = typeof(CreateTestProductDto);

        // Act
        var hasCreatedBy = dtoType.GetProperty("CreatedById") != null;
        var hasCreatedOn = dtoType.GetProperty("CreatedOn") != null;
        var hasModifiedBy = dtoType.GetProperty("ModifiedById") != null;
        var hasModifiedOn = dtoType.GetProperty("ModifiedOn") != null;

        // Assert
        Assert.False(hasCreatedBy, "Create DTOs should not have CreatedById");
        Assert.False(hasCreatedOn, "Create DTOs should not have CreatedOn");
        Assert.False(hasModifiedBy, "Create DTOs should not have ModifiedById");
        Assert.False(hasModifiedOn, "Create DTOs should not have ModifiedOn");
    }

    [Fact]
    public void CreateDto_Should_HaveBusinessProperties()
    {
        // Arrange
        var dtoType = typeof(CreateTestProductDto);

        // Act
        var hasName = dtoType.GetProperty("Name") != null;
        var hasPrice = dtoType.GetProperty("Price") != null;

        // Assert
        Assert.True(hasName, "Create DTOs should have business properties");
        Assert.True(hasPrice, "Create DTOs should have business properties");
    }

    #endregion

    #region Design Pattern Tests

    [Fact]
    public void ICreateDto_FollowsMarkerPattern()
    {
        // Arrange
        var interfaceType = typeof(ICreateDto);

        // Act
        var hasProperties = interfaceType.GetProperties().Any();
        var hasMethods = interfaceType.GetMethods().Any(m => !m.IsSpecialName); // Exclude property accessors

        // Assert
        Assert.False(hasProperties, "Marker interface should not have properties");
        Assert.False(hasMethods, "Marker interface should not have methods");
    }

    [Fact]
    public void ICreateDto_ProvidesTypeIdentification()
    {
        // Arrange
        var dto1 = new CreateTestProductDto { Name = "Test", Price = 10.00m };
        var dto2 = new CreateTestOrderDto { OrderDate = DateTime.UtcNow };

        // Act
        var type1 = dto1.GetType();
        var type2 = dto2.GetType();
        var bothImplementICreateDto = typeof(ICreateDto).IsAssignableFrom(type1) 
            && typeof(ICreateDto).IsAssignableFrom(type2);

        // Assert
        Assert.True(bothImplementICreateDto);
    }

    [Fact]
    public void ICreateDto_IsPublic()
    {
        // Arrange
        var interfaceType = typeof(ICreateDto);

        // Act
        var isPublic = interfaceType.IsPublic;

        // Assert
        Assert.True(isPublic, "ICreateDto should be public");
    }

    #endregion

    #region Usage Scenario Tests

    [Fact]
    public void CreateDto_CanRepresent_NewEntityData()
    {
        // Arrange & Act - Simulating data from client for entity creation
        var dto = new CreateTestProductDto
        {
            Name = "New Product",
            Price = 29.99m,
            Description = "A brand new product"
        };

        // Assert
        Assert.Equal("New Product", dto.Name);
        Assert.Equal(29.99m, dto.Price);
        Assert.Equal("A brand new product", dto.Description);
    }

    [Fact]
    public void CreateDto_SupportsObjectInitializer()
    {
        // Arrange & Act
        var dto = new CreateTestProductDto
        {
            Name = "Initialized Product",
            Price = 49.99m
        };

        // Assert
        Assert.Equal("Initialized Product", dto.Name);
        Assert.Equal(49.99m, dto.Price);
    }

    [Fact]
    public void CreateDto_CanBeUsedIn_GenericMethods()
    {
        // Arrange
        var dto = new CreateTestProductDto { Name = "Test", Price = 10.00m };

        // Act
        var result = ProcessCreateDto(dto);

        // Assert
        Assert.True(result);
    }

    // Helper method to demonstrate generic usage
    private bool ProcessCreateDto<T>(T dto) where T : ICreateDto
    {
        return dto != null;
    }

    #endregion
}
