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
using System.Linq;
using JumpStart.Api.DTOs;
using JumpStart.Api.DTOs.Advanced;
using Xunit;

namespace JumpStart.Tests.Api.DTOs;

/// <summary>
/// Unit tests for the <see cref="SimpleEntityDto"/> class.
/// Tests inheritance, Guid identifier handling, simplification pattern, and usage scenarios.
/// </summary>
public class SimpleEntityDtoTests
{
    #region Test DTOs

    /// <summary>
    /// Simple test DTO for testing.
    /// </summary>
    public class TestSimpleDto : SimpleEntityDto
    {
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }

    /// <summary>
    /// Another test DTO for polymorphism tests.
    /// </summary>
    public class TestProductDto : SimpleEntityDto
    {
        public string ProductName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
    }

    /// <summary>
    /// Test DTO with related entities.
    /// </summary>
    public class TestCustomerDto : SimpleEntityDto
    {
        public string CustomerName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    #endregion

    #region Property Tests

    [Fact]
    public void Id_CanBeSet_AndRetrieved()
    {
        // Arrange
        var dto = new TestSimpleDto();
        var id = Guid.NewGuid();

        // Act
        dto.Id = id;

        // Assert
        Assert.Equal(id, dto.Id);
    }

    [Fact]
    public void Id_IsGuidType()
    {
        // Arrange
        var dto = new TestSimpleDto();
        var id = Guid.NewGuid();

        // Act
        dto.Id = id;

        // Assert
        Assert.IsType<Guid>(dto.Id);
    }

    [Fact]
    public void Id_DefaultValue_IsGuidEmpty()
    {
        // Arrange & Act
        var dto = new TestSimpleDto();

        // Assert
        Assert.Equal(Guid.Empty, dto.Id);
    }

    [Fact]
    public void Properties_CanBeSet_AndRetrieved()
    {
        // Arrange
        var dto = new TestSimpleDto();
        var id = Guid.NewGuid();

        // Act
        dto.Id = id;
        dto.Name = "Test Product";
        dto.Price = 99.99m;

        // Assert
        Assert.Equal(id, dto.Id);
        Assert.Equal("Test Product", dto.Name);
        Assert.Equal(99.99m, dto.Price);
    }

    #endregion

    #region Inheritance Tests

    [Fact]
    public void SimpleEntityDto_InheritsFrom_EntityDto()
    {
        // Arrange
        var dtoType = typeof(SimpleEntityDto);

        // Act
        var baseType = dtoType.BaseType;

        // Assert
        Assert.NotNull(baseType);
        Assert.True(baseType!.IsGenericType);
        Assert.Equal(typeof(EntityDto<>), baseType.GetGenericTypeDefinition());
    }

    [Fact]
    public void SimpleEntityDto_UsesGuid_AsKeyType()
    {
        // Arrange
        var dtoType = typeof(SimpleEntityDto);

        // Act
        var baseType = dtoType.BaseType;
        var genericArguments = baseType!.GetGenericArguments();

        // Assert
        Assert.Single(genericArguments);
        Assert.Equal(typeof(Guid), genericArguments[0]);
    }

    [Fact]
    public void SimpleEntityDto_ImplementsIDto_ThroughInheritance()
    {
        // Arrange
        var dto = new TestSimpleDto();

        // Act
        var isDto = dto is IDto;

        // Assert
        Assert.True(isDto);
    }

    [Fact]
    public void SimpleEntityDto_HasIdProperty_FromBase()
    {
        // Arrange
        var dto = new TestSimpleDto();
        var id = Guid.NewGuid();

        // Act
        dto.Id = id;

        // Assert
        Assert.Equal(id, dto.Id);
    }

    #endregion

    #region Simplification Pattern Tests

    [Fact]
    public void SimpleEntityDto_SimplifiesGenericSignature()
    {
        // Arrange
        var simpleType = typeof(SimpleEntityDto);
        var advancedType = typeof(EntityDto<Guid>);

        // Act
        var simpleIsGeneric = simpleType.IsGenericType;
        var advancedIsGeneric = advancedType.IsGenericType;

        // Assert
        Assert.False(simpleIsGeneric, "SimpleEntityDto should not be generic (closed type)");
        Assert.True(advancedIsGeneric, "EntityDto<Guid> should be generic");
    }

    [Fact]
    public void SimpleEntityDto_EliminatesGenericParameter()
    {
        // Arrange & Act
        // This demonstrates that SimpleEntityDto usage doesn't require generic parameter
        var dto = new TestSimpleDto(); // No <Guid> needed
        dto.Id = Guid.NewGuid();

        // Assert
        Assert.NotEqual(Guid.Empty, dto.Id);
    }

    [Fact]
    public void SimpleEntityDto_ProvidesCleaner_Syntax()
    {
        // Arrange - Compare declaration complexity
        var simpleTypeName = typeof(TestSimpleDto).BaseType!.Name;
        var expectedName = "SimpleEntityDto";

        // Act
        var actualBaseName = typeof(SimpleEntityDto).Name;

        // Assert
        Assert.Equal(expectedName, actualBaseName);
        Assert.DoesNotContain("`", actualBaseName); // No generic arity marker
    }

    #endregion

    #region Class Characteristics Tests

    [Fact]
    public void SimpleEntityDto_IsAbstract()
    {
        // Arrange
        var dtoType = typeof(SimpleEntityDto);

        // Act
        var isAbstract = dtoType.IsAbstract;

        // Assert
        Assert.True(isAbstract, "SimpleEntityDto should be abstract");
    }

    [Fact]
    public void SimpleEntityDto_IsInCorrectNamespace()
    {
        // Arrange
        var dtoType = typeof(SimpleEntityDto);

        // Act
        var namespaceName = dtoType.Namespace;

        // Assert
        Assert.Equal("JumpStart.Api.DTOs", namespaceName);
    }

    [Fact]
    public void SimpleEntityDto_HasNoAdditionalMembers()
    {
        // Arrange
        var dtoType = typeof(SimpleEntityDto);

        // Act
        var declaredMembers = dtoType.GetMembers(System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

        // Assert
        Assert.Empty(declaredMembers); // Should only inherit from EntityDto<Guid>
    }

    #endregion

    #region Guid Identifier Tests

    [Fact]
    public void SimpleEntityDto_SupportsNewGuid_Generation()
    {
        // Arrange
        var dto = new TestSimpleDto();

        // Act
        dto.Id = Guid.NewGuid();

        // Assert
        Assert.NotEqual(Guid.Empty, dto.Id);
    }

    [Fact]
    public void SimpleEntityDto_SupportsGuid_Parsing()
    {
        // Arrange
        var dto = new TestSimpleDto();
        var guidString = "3fa85f64-5717-4562-b3fc-2c963f66afa6";

        // Act
        dto.Id = Guid.Parse(guidString);

        // Assert
        Assert.Equal(Guid.Parse(guidString), dto.Id);
    }

    [Fact]
    public void SimpleEntityDto_SupportsGuid_Comparison()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto1 = new TestSimpleDto { Id = id };
        var dto2 = new TestSimpleDto { Id = id };

        // Act
        var idsMatch = dto1.Id == dto2.Id;

        // Assert
        Assert.True(idsMatch);
    }

    [Fact]
    public void SimpleEntityDto_GuidsAre_GloballyUnique()
    {
        // Arrange & Act
        var dto1 = new TestSimpleDto { Id = Guid.NewGuid() };
        var dto2 = new TestSimpleDto { Id = Guid.NewGuid() };

        // Assert
        Assert.NotEqual(dto1.Id, dto2.Id);
    }

    #endregion

    #region Concrete Implementation Tests

    [Fact]
    public void ConcreteDto_CanInherit_FromSimpleEntityDto()
    {
        // Arrange
        var concreteType = typeof(TestSimpleDto);

        // Act
        var baseType = concreteType.BaseType;

        // Assert
        Assert.NotNull(baseType);
        Assert.Equal(typeof(SimpleEntityDto), baseType);
    }

    [Fact]
    public void ConcreteDto_InheritsIdProperty()
    {
        // Arrange
        var concreteType = typeof(TestSimpleDto);

        // Act
        var idProperty = concreteType.GetProperty(nameof(SimpleEntityDto.Id));

        // Assert
        Assert.NotNull(idProperty);
        Assert.Equal(typeof(Guid), idProperty!.PropertyType);
    }

    [Fact]
    public void ConcreteDto_CanAddOwnProperties()
    {
        // Arrange
        var concreteType = typeof(TestSimpleDto);

        // Act
        var ownProperties = concreteType.GetProperties()
            .Where(p => p.DeclaringType == concreteType)
            .ToList();

        // Assert
        Assert.Contains(ownProperties, p => p.Name == "Name");
        Assert.Contains(ownProperties, p => p.Name == "Price");
    }

    #endregion

    #region Usage Scenario Tests

    [Fact]
    public void SimpleEntityDto_CanRepresent_ApiResponse()
    {
        // Arrange & Act - Simulating API response
        var dto = new TestProductDto
        {
            Id = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
            ProductName = "Laptop",
            Category = "Electronics"
        };

        // Assert
        Assert.Equal(Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"), dto.Id);
        Assert.Equal("Laptop", dto.ProductName);
        Assert.Equal("Electronics", dto.Category);
    }

    [Fact]
    public void SimpleEntityDto_SupportsObjectInitializer()
    {
        // Arrange & Act
        var dto = new TestSimpleDto
        {
            Id = Guid.NewGuid(),
            Name = "Initialized Product",
            Price = 49.99m
        };

        // Assert
        Assert.NotEqual(Guid.Empty, dto.Id);
        Assert.Equal("Initialized Product", dto.Name);
        Assert.Equal(49.99m, dto.Price);
    }

    [Fact]
    public void SimpleEntityDto_CanBeUsed_InCollections()
    {
        // Arrange
        var dtos = new System.Collections.Generic.List<TestSimpleDto>
        {
            new TestSimpleDto { Id = Guid.NewGuid(), Name = "Product1", Price = 10.00m },
            new TestSimpleDto { Id = Guid.NewGuid(), Name = "Product2", Price = 20.00m }
        };

        // Act
        var allHaveUniqueIds = dtos.Select(d => d.Id).Distinct().Count() == dtos.Count;

        // Assert
        Assert.Equal(2, dtos.Count);
        Assert.True(allHaveUniqueIds);
    }

    [Fact]
    public void SimpleEntityDto_CanBeUsed_WithLinq()
    {
        // Arrange
        var dtos = new[]
        {
            new TestSimpleDto { Id = Guid.NewGuid(), Name = "Product A", Price = 10.00m },
            new TestSimpleDto { Id = Guid.NewGuid(), Name = "Product B", Price = 20.00m },
            new TestSimpleDto { Id = Guid.NewGuid(), Name = "Product C", Price = 30.00m }
        };

        // Act
        var expensiveProducts = dtos.Where(d => d.Price > 15.00m).ToList();

        // Assert
        Assert.Equal(2, expensiveProducts.Count);
    }

    #endregion

    #region Type Safety Tests

    [Fact]
    public void SimpleEntityDto_EnforcesGuidType_AtCompileTime()
    {
        // Arrange
        var dto = new TestSimpleDto();

        // Act - This should compile with Guid
        dto.Id = Guid.NewGuid();

        // Assert
        Assert.IsType<Guid>(dto.Id);
    }

    [Fact]
    public void SimpleEntityDto_CanBeAssigned_ToIDto()
    {
        // Arrange
        var dto = new TestSimpleDto { Id = Guid.NewGuid(), Name = "Test" };

        // Act
        IDto idto = dto;

        // Assert
        Assert.NotNull(idto);
        Assert.IsAssignableFrom<IDto>(dto);
    }

    [Fact]
    public void SimpleEntityDto_CanBeIdentified_AtRuntime()
    {
        // Arrange
        object obj = new TestSimpleDto { Id = Guid.NewGuid() };

        // Act
        var isSimpleEntityDto = obj is SimpleEntityDto;

        // Assert
        Assert.True(isSimpleEntityDto);
    }

    #endregion

    #region Comparison with Advanced DTOs Tests

    [Fact]
    public void SimpleEntityDto_IsSimpler_ThanEntityDto()
    {
        // Arrange
        var simpleType = typeof(SimpleEntityDto);
        var advancedType = typeof(EntityDto<Guid>);

        // Act
        var simpleHasGenericParams = simpleType.GetGenericArguments().Any();
        var advancedHasGenericParams = advancedType.GetGenericArguments().Any();

        // Assert
        Assert.False(simpleHasGenericParams, "SimpleEntityDto is a closed type");
        Assert.True(advancedHasGenericParams, "EntityDto<Guid> has generic parameters");
    }

    [Fact]
    public void SimpleEntityDto_EquivalentTo_EntityDtoGuid()
    {
        // Arrange
        var simpleBaseType = typeof(SimpleEntityDto).BaseType;

        // Act
        var isEntityDtoGuid = simpleBaseType != null && 
                              simpleBaseType.IsGenericType &&
                              simpleBaseType.GetGenericTypeDefinition() == typeof(EntityDto<>) &&
                              simpleBaseType.GetGenericArguments()[0] == typeof(Guid);

        // Assert
        Assert.True(isEntityDtoGuid, "SimpleEntityDto should inherit from EntityDto<Guid>");
    }

    #endregion
}
