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

namespace JumpStart.Tests.Api.DTOs.Advanced;

/// <summary>
/// Unit tests for the <see cref="EntityDto{TKey}"/> class.
/// Tests property functionality, inheritance, interface implementation, and support for various key types.
/// </summary>
public class EntityDtoTests
{
    #region Test DTOs

    /// <summary>
    /// Concrete test DTO with int key type.
    /// </summary>
    public class TestIntDto : EntityDto<int>
    {
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }

    /// <summary>
    /// Concrete test DTO with long key type.
    /// </summary>
    public class TestLongDto : EntityDto<long>
    {
        public string Description { get; set; } = string.Empty;
    }

    /// <summary>
    /// Concrete test DTO with Guid key type.
    /// </summary>
    public class TestGuidDto : EntityDto<Guid>
    {
        public string Title { get; set; } = string.Empty;
    }

    /// <summary>
    /// Concrete test DTO with custom struct key type.
    /// </summary>
    public struct CustomKey
    {
        public int Id { get; set; }
        public int Version { get; set; }
    }

    /// <summary>
    /// Test DTO with custom struct key.
    /// </summary>
    public class TestCustomKeyDto : EntityDto<CustomKey>
    {
        public string Data { get; set; } = string.Empty;
    }

    #endregion

    #region Id Property Tests

    [Fact]
    public void Id_CanBeSet_WithIntType()
    {
        // Arrange
        var dto = new TestIntDto();

        // Act
        dto.Id = 42;

        // Assert
        Assert.Equal(42, dto.Id);
    }

    [Fact]
    public void Id_CanBeSet_WithLongType()
    {
        // Arrange
        var dto = new TestLongDto();

        // Act
        dto.Id = 9876543210L;

        // Assert
        Assert.Equal(9876543210L, dto.Id);
    }

    [Fact]
    public void Id_CanBeSet_WithGuidType()
    {
        // Arrange
        var dto = new TestGuidDto();
        var guid = Guid.NewGuid();

        // Act
        dto.Id = guid;

        // Assert
        Assert.Equal(guid, dto.Id);
    }

    [Fact]
    public void Id_CanBeSet_WithCustomStructType()
    {
        // Arrange
        var dto = new TestCustomKeyDto();
        var customKey = new CustomKey { Id = 1, Version = 2 };

        // Act
        dto.Id = customKey;

        // Assert
        Assert.Equal(1, dto.Id.Id);
        Assert.Equal(2, dto.Id.Version);
    }

    [Fact]
    public void Id_DefaultValue_IsDefaultForType()
    {
        // Arrange & Act
        var intDto = new TestIntDto();
        var longDto = new TestLongDto();
        var guidDto = new TestGuidDto();

        // Assert
        Assert.Equal(0, intDto.Id);
        Assert.Equal(0L, longDto.Id);
        Assert.Equal(Guid.Empty, guidDto.Id);
    }

    [Fact]
    public void Id_CanBeReadAndWritten()
    {
        // Arrange
        var dto = new TestIntDto();

        // Act
        dto.Id = 100;
        var retrievedId = dto.Id;

        // Assert
        Assert.Equal(100, retrievedId);
    }

    #endregion

    #region Interface Implementation Tests

    [Fact]
    public void EntityDto_Implements_IDto()
    {
        // Arrange
        var dtoType = typeof(EntityDto<int>);

        // Act
        var implementsIDto = typeof(IDto).IsAssignableFrom(dtoType);

        // Assert
        Assert.True(implementsIDto, "EntityDto should implement IDto");
    }

    [Fact]
    public void EntityDto_CanBeAssigned_ToIDto()
    {
        // Arrange
        var dto = new TestIntDto { Id = 123 };

        // Act
        IDto idto = dto;

        // Assert
        Assert.NotNull(idto);
        Assert.IsAssignableFrom<IDto>(dto);
    }

    #endregion

    #region Type Constraint Tests

    [Fact]
    public void EntityDto_RequiresStructConstraint()
    {
        // Arrange
        var dtoType = typeof(EntityDto<>);
        var genericParameter = dtoType.GetGenericArguments()[0];

        // Act
        var hasValueTypeConstraint = (genericParameter.GenericParameterAttributes & System.Reflection.GenericParameterAttributes.NotNullableValueTypeConstraint) != 0;

        // Assert
        Assert.True(hasValueTypeConstraint, "TKey should have struct constraint");
    }

    [Fact]
    public void EntityDto_WorksWithIntKey()
    {
        // Arrange & Act
        var dto = new TestIntDto
        {
            Id = 50,
            Name = "Test Product",
            Price = 99.99m
        };

        // Assert
        Assert.Equal(50, dto.Id);
        Assert.IsType<int>(dto.Id);
        Assert.Equal("Test Product", dto.Name);
        Assert.Equal(99.99m, dto.Price);
    }

    [Fact]
    public void EntityDto_WorksWithLongKey()
    {
        // Arrange & Act
        var dto = new TestLongDto
        {
            Id = 1234567890123L,
            Description = "Large scale entity"
        };

        // Assert
        Assert.Equal(1234567890123L, dto.Id);
        Assert.IsType<long>(dto.Id);
        Assert.Equal("Large scale entity", dto.Description);
    }

    [Fact]
    public void EntityDto_WorksWithGuidKey()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var dto = new TestGuidDto
        {
            Id = guid,
            Title = "Guid-based entity"
        };

        // Assert
        Assert.Equal(guid, dto.Id);
        Assert.IsType<Guid>(dto.Id);
        Assert.Equal("Guid-based entity", dto.Title);
    }

    #endregion

    #region Class Characteristics Tests

    [Fact]
    public void EntityDto_IsAbstract()
    {
        // Arrange
        var dtoType = typeof(EntityDto<int>);

        // Act
        var isAbstract = dtoType.IsAbstract;

        // Assert
        Assert.True(isAbstract, "EntityDto should be abstract");
    }

    [Fact]
    public void EntityDto_IsInCorrectNamespace()
    {
        // Arrange
        var dtoType = typeof(EntityDto<>);

        // Act
        var namespaceName = dtoType.Namespace;

        // Assert
        Assert.Equal("JumpStart.Api.DTOs.Advanced", namespaceName);
    }

    [Fact]
    public void EntityDto_HasPublicIdProperty()
    {
        // Arrange
        var dtoType = typeof(EntityDto<int>);

        // Act
        var idProperty = dtoType.GetProperty(nameof(EntityDto<int>.Id));

        // Assert
        Assert.NotNull(idProperty);
        Assert.True(idProperty!.CanRead);
        Assert.True(idProperty.CanWrite);
        Assert.True(idProperty.GetMethod!.IsPublic);
        Assert.True(idProperty.SetMethod!.IsPublic);
    }

    [Fact]
    public void EntityDto_HasOnlyIdProperty()
    {
        // Arrange
        var dtoType = typeof(EntityDto<int>);

        // Act
        var properties = dtoType.GetProperties()
            .Where(p => p.DeclaringType == dtoType)
            .ToList();

        // Assert
        Assert.Single(properties);
        Assert.Equal(nameof(EntityDto<int>.Id), properties[0].Name);
    }

    #endregion

    #region Inheritance Chain Tests

    [Fact]
    public void ConcreteDto_CanInherit_FromEntityDto()
    {
        // Arrange
        var concreteType = typeof(TestIntDto);

        // Act
        var baseType = concreteType.BaseType;

        // Assert
        Assert.NotNull(baseType);
        Assert.True(baseType!.IsGenericType);
        Assert.Equal(typeof(EntityDto<>), baseType.GetGenericTypeDefinition());
    }

    [Fact]
    public void ConcreteDto_InheritsIdProperty()
    {
        // Arrange
        var concreteType = typeof(TestIntDto);

        // Act
        var idProperty = concreteType.GetProperty(nameof(EntityDto<int>.Id));

        // Assert
        Assert.NotNull(idProperty);
        Assert.Equal(typeof(EntityDto<int>), idProperty!.DeclaringType);
    }

    [Fact]
    public void ConcreteDto_CanAddOwnProperties()
    {
        // Arrange
        var concreteType = typeof(TestIntDto);

        // Act
        var ownProperties = concreteType.GetProperties()
            .Where(p => p.DeclaringType == concreteType)
            .ToList();

        // Assert
        Assert.Contains(ownProperties, p => p.Name == "Name");
        Assert.Contains(ownProperties, p => p.Name == "Price");
    }

    #endregion

    #region Equality and Identity Tests

    [Fact]
    public void TwoDtos_WithSameId_AreNotEqual_ByDefault()
    {
        // Arrange
        var dto1 = new TestIntDto { Id = 1, Name = "First" };
        var dto2 = new TestIntDto { Id = 1, Name = "First" };

        // Act
        var areEqual = dto1.Equals(dto2);
        var areReferenceEqual = ReferenceEquals(dto1, dto2);

        // Assert
        Assert.False(areReferenceEqual, "Different instances should not be reference equal");
        Assert.False(areEqual, "DTOs use reference equality by default");
    }

    [Fact]
    public void SameDto_IsReferenceEqual_ToItself()
    {
        // Arrange
        var dto = new TestIntDto { Id = 1 };

        // Act
        var areEqual = dto.Equals(dto);
        var areReferenceEqual = ReferenceEquals(dto, dto);

        // Assert
        Assert.True(areReferenceEqual);
        Assert.True(areEqual);
    }

    [Fact]
    public void Id_CanBeUsed_AsKeyInDictionary()
    {
        // Arrange
        var dto1 = new TestIntDto { Id = 1, Name = "First" };
        var dto2 = new TestIntDto { Id = 2, Name = "Second" };
        var dictionary = new System.Collections.Generic.Dictionary<int, TestIntDto>();

        // Act
        dictionary[dto1.Id] = dto1;
        dictionary[dto2.Id] = dto2;

        // Assert
        Assert.Equal(2, dictionary.Count);
        Assert.Equal("First", dictionary[1].Name);
        Assert.Equal("Second", dictionary[2].Name);
    }

    #endregion

    #region Usage Scenario Tests

    [Fact]
    public void EntityDto_CanRepresent_DatabaseEntity()
    {
        // Arrange & Act - Simulating retrieval from database
        var dto = new TestIntDto
        {
            Id = 123,
            Name = "Product Name",
            Price = 49.99m
        };

        // Assert
        Assert.Equal(123, dto.Id);
        Assert.Equal("Product Name", dto.Name);
        Assert.Equal(49.99m, dto.Price);
    }

    [Fact]
    public void EntityDto_SupportsObjectInitializer()
    {
        // Arrange & Act
        var dto = new TestIntDto
        {
            Id = 999,
            Name = "Initialized",
            Price = 100.00m
        };

        // Assert
        Assert.Equal(999, dto.Id);
        Assert.Equal("Initialized", dto.Name);
        Assert.Equal(100.00m, dto.Price);
    }

    [Fact]
    public void EntityDto_CanBeSerializedPattern()
    {
        // Arrange
        var dto = new TestIntDto
        {
            Id = 42,
            Name = "Serializable",
            Price = 25.50m
        };

        // Act - Verify all properties are accessible for serialization
        var id = dto.Id;
        var name = dto.Name;
        var price = dto.Price;

        // Assert
        Assert.Equal(42, id);
        Assert.Equal("Serializable", name);
        Assert.Equal(25.50m, price);
    }

    #endregion

    #region Generic Type Parameter Tests

    [Fact]
    public void EntityDto_HasOneGenericParameter()
    {
        // Arrange
        var dtoType = typeof(EntityDto<>);

        // Act
        var genericParameters = dtoType.GetGenericArguments();

        // Assert
        Assert.Single(genericParameters);
        Assert.Equal("TKey", genericParameters[0].Name);
    }

    [Fact]
    public void EntityDto_GenericParameter_MatchesIdType()
    {
        // Arrange
        var intDtoType = typeof(EntityDto<int>);
        var longDtoType = typeof(EntityDto<long>);
        var guidDtoType = typeof(EntityDto<Guid>);

        // Act
        var intIdType = intDtoType.GetProperty("Id")!.PropertyType;
        var longIdType = longDtoType.GetProperty("Id")!.PropertyType;
        var guidIdType = guidDtoType.GetProperty("Id")!.PropertyType;

        // Assert
        Assert.Equal(typeof(int), intIdType);
        Assert.Equal(typeof(long), longIdType);
        Assert.Equal(typeof(Guid), guidIdType);
    }

    #endregion
}
