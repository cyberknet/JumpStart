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
using JumpStart.Api.DTOs.Advanced;
using Xunit;

namespace JumpStart.Tests.Api.DTOs.Advanced;

/// <summary>
/// Unit tests for the <see cref="AuditableEntityDto{TKey}"/> class.
/// Tests property initialization, inheritance, audit field handling, and various key types.
/// </summary>
public class AuditableEntityDtoTests
{
    #region Test DTOs

    /// <summary>
    /// Concrete test DTO with int key type.
    /// </summary>
    public class TestAuditableDto : AuditableEntityDto<int>
    {
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }

    /// <summary>
    /// Concrete test DTO with long key type.
    /// </summary>
    public class TestAuditableLongDto : AuditableEntityDto<long>
    {
        public string Description { get; set; } = string.Empty;
    }

    /// <summary>
    /// Concrete test DTO with Guid key type.
    /// </summary>
    public class TestAuditableGuidDto : AuditableEntityDto<Guid>
    {
        public string Title { get; set; } = string.Empty;
    }

    #endregion

    #region Property Tests

    [Fact]
    public void Properties_CanBeSet_AndRetrieved()
    {
        // Arrange
        var dto = new TestAuditableDto();
        var now = DateTime.UtcNow;

        // Act
        dto.Id = 1;
        dto.Name = "Test";
        dto.Price = 99.99m;
        dto.CreatedById = 10;
        dto.CreatedOn = now;
        dto.ModifiedById = 20;
        dto.ModifiedOn = now.AddHours(1);

        // Assert
        Assert.Equal(1, dto.Id);
        Assert.Equal("Test", dto.Name);
        Assert.Equal(99.99m, dto.Price);
        Assert.Equal(10, dto.CreatedById);
        Assert.Equal(now, dto.CreatedOn);
        Assert.Equal(20, dto.ModifiedById);
        Assert.Equal(now.AddHours(1), dto.ModifiedOn);
    }

    [Fact]
    public void CreatedById_WithIntKey_StoresCorrectValue()
    {
        // Arrange
        var dto = new TestAuditableDto();

        // Act
        dto.CreatedById = 42;

        // Assert
        Assert.Equal(42, dto.CreatedById);
    }

    [Fact]
    public void CreatedOn_StoresUtcDateTime()
    {
        // Arrange
        var dto = new TestAuditableDto();
        var utcNow = DateTime.UtcNow;

        // Act
        dto.CreatedOn = utcNow;

        // Assert
        Assert.Equal(utcNow, dto.CreatedOn);
        Assert.Equal(DateTimeKind.Utc, dto.CreatedOn.Kind);
    }

    [Fact]
    public void ModifiedById_CanBeNull()
    {
        // Arrange
        var dto = new TestAuditableDto
        {
            CreatedById = 1,
            CreatedOn = DateTime.UtcNow
        };

        // Act
        dto.ModifiedById = null;

        // Assert
        Assert.Null(dto.ModifiedById);
    }

    [Fact]
    public void ModifiedById_CanHaveValue()
    {
        // Arrange
        var dto = new TestAuditableDto();

        // Act
        dto.ModifiedById = 15;

        // Assert
        Assert.NotNull(dto.ModifiedById);
        Assert.Equal(15, dto.ModifiedById.Value);
    }

    [Fact]
    public void ModifiedOn_CanBeNull()
    {
        // Arrange
        var dto = new TestAuditableDto
        {
            CreatedById = 1,
            CreatedOn = DateTime.UtcNow
        };

        // Act
        dto.ModifiedOn = null;

        // Assert
        Assert.Null(dto.ModifiedOn);
    }

    [Fact]
    public void ModifiedOn_CanHaveValue()
    {
        // Arrange
        var dto = new TestAuditableDto();
        var modifiedDate = DateTime.UtcNow;

        // Act
        dto.ModifiedOn = modifiedDate;

        // Assert
        Assert.NotNull(dto.ModifiedOn);
        Assert.Equal(modifiedDate, dto.ModifiedOn.Value);
    }

    #endregion

    #region Inheritance Tests

    [Fact]
    public void AuditableEntityDto_InheritsFrom_EntityDto()
    {
        // Arrange
        var dtoType = typeof(AuditableEntityDto<int>);

        // Act
        var baseType = dtoType.BaseType;

        // Assert
        Assert.NotNull(baseType);
        Assert.True(baseType!.IsGenericType);
        Assert.Equal(typeof(EntityDto<>), baseType.GetGenericTypeDefinition());
    }

    [Fact]
    public void AuditableEntityDto_HasIdProperty_FromBase()
    {
        // Arrange
        var dto = new TestAuditableDto();

        // Act
        dto.Id = 123;

        // Assert
        Assert.Equal(123, dto.Id);
    }

    [Fact]
    public void AuditableEntityDto_HasCorrectPropertyCount()
    {
        // Arrange
        var dtoType = typeof(TestAuditableDto);

        // Act - Get public instance properties
        var properties = dtoType.GetProperties().Where(p => p.DeclaringType == typeof(AuditableEntityDto<int>)).ToList();

        // Assert - Should have 4 audit properties
        Assert.Equal(4, properties.Count);
        Assert.Contains(properties, p => p.Name == nameof(AuditableEntityDto<int>.CreatedById));
        Assert.Contains(properties, p => p.Name == nameof(AuditableEntityDto<int>.CreatedOn));
        Assert.Contains(properties, p => p.Name == nameof(AuditableEntityDto<int>.ModifiedById));
        Assert.Contains(properties, p => p.Name == nameof(AuditableEntityDto<int>.ModifiedOn));
    }

    #endregion

    #region Type Constraint Tests

    [Fact]
    public void AuditableEntityDto_RequiresStructConstraint()
    {
        // Arrange
        var dtoType = typeof(AuditableEntityDto<>);
        var genericParameter = dtoType.GetGenericArguments()[0];

        // Act
        var constraints = genericParameter.GetGenericParameterConstraints();
        var hasValueTypeConstraint = (genericParameter.GenericParameterAttributes & System.Reflection.GenericParameterAttributes.NotNullableValueTypeConstraint) != 0;

        // Assert
        Assert.True(hasValueTypeConstraint, "TKey should have struct constraint");
    }

    [Fact]
    public void AuditableEntityDto_WorksWithIntKey()
    {
        // Arrange & Act
        var dto = new TestAuditableDto
        {
            Id = 1,
            CreatedById = 10,
            CreatedOn = DateTime.UtcNow
        };

        // Assert
        Assert.Equal(1, dto.Id);
        Assert.Equal(10, dto.CreatedById);
        Assert.IsType<int>(dto.Id);
        Assert.IsType<int>(dto.CreatedById);
    }

    [Fact]
    public void AuditableEntityDto_WorksWithLongKey()
    {
        // Arrange & Act
        var dto = new TestAuditableLongDto
        {
            Id = 1000000000L,
            CreatedById = 5000000000L,
            CreatedOn = DateTime.UtcNow
        };

        // Assert
        Assert.Equal(1000000000L, dto.Id);
        Assert.Equal(5000000000L, dto.CreatedById);
        Assert.IsType<long>(dto.Id);
        Assert.IsType<long>(dto.CreatedById);
    }

    [Fact]
    public void AuditableEntityDto_WorksWithGuidKey()
    {
        // Arrange
        var id = Guid.NewGuid();
        var creatorId = Guid.NewGuid();

        // Act
        var dto = new TestAuditableGuidDto
        {
            Id = id,
            CreatedById = creatorId,
            CreatedOn = DateTime.UtcNow
        };

        // Assert
        Assert.Equal(id, dto.Id);
        Assert.Equal(creatorId, dto.CreatedById);
        Assert.IsType<Guid>(dto.Id);
        Assert.IsType<Guid>(dto.CreatedById);
    }

    #endregion

    #region Audit Scenario Tests

    [Fact]
    public void NewEntity_HasCreationAuditFields_NoModificationFields()
    {
        // Arrange
        var createdDate = new DateTime(2026, 1, 15, 10, 30, 0, DateTimeKind.Utc);

        // Act - Simulate a newly created entity
        var dto = new TestAuditableDto
        {
            Id = 1,
            Name = "New Entity",
            CreatedById = 1,
            CreatedOn = createdDate,
            ModifiedById = null,
            ModifiedOn = null
        };

        // Assert
        Assert.Equal(1, dto.CreatedById);
        Assert.Equal(createdDate, dto.CreatedOn);
        Assert.Null(dto.ModifiedById);
        Assert.Null(dto.ModifiedOn);
    }

    [Fact]
    public void ModifiedEntity_HasBothCreationAndModificationFields()
    {
        // Arrange
        var createdDate = new DateTime(2026, 1, 15, 10, 30, 0, DateTimeKind.Utc);
        var modifiedDate = new DateTime(2026, 1, 20, 14, 45, 0, DateTimeKind.Utc);

        // Act - Simulate a modified entity
        var dto = new TestAuditableDto
        {
            Id = 1,
            Name = "Modified Entity",
            CreatedById = 1,
            CreatedOn = createdDate,
            ModifiedById = 5,
            ModifiedOn = modifiedDate
        };

        // Assert
        Assert.Equal(1, dto.CreatedById);
        Assert.Equal(createdDate, dto.CreatedOn);
        Assert.Equal(5, dto.ModifiedById);
        Assert.Equal(modifiedDate, dto.ModifiedOn);
        Assert.True(dto.ModifiedOn > dto.CreatedOn);
    }

    [Fact]
    public void SelfModifiedEntity_CanHaveSameCreatorAndModifier()
    {
        // Arrange
        var createdDate = new DateTime(2026, 1, 15, 10, 30, 0, DateTimeKind.Utc);
        var modifiedDate = new DateTime(2026, 1, 15, 11, 00, 0, DateTimeKind.Utc);

        // Act - User 1 creates and modifies their own entity
        var dto = new TestAuditableDto
        {
            Id = 1,
            Name = "Self-Modified Entity",
            CreatedById = 1,
            CreatedOn = createdDate,
            ModifiedById = 1,
            ModifiedOn = modifiedDate
        };

        // Assert
        Assert.Equal(dto.CreatedById, dto.ModifiedById);
        Assert.NotEqual(dto.CreatedOn, dto.ModifiedOn);
    }

    [Fact]
    public void AuditFields_AreIndependent_FromEntityData()
    {
        // Arrange & Act
        var dto = new TestAuditableDto
        {
            Id = 100,
            Name = "Product",
            Price = 50.00m,
            CreatedById = 10,
            CreatedOn = DateTime.UtcNow,
            ModifiedById = 20,
            ModifiedOn = DateTime.UtcNow.AddDays(1)
        };

        // Assert - Audit fields don't affect entity data
        Assert.NotEqual(dto.Id, dto.CreatedById);
        Assert.NotEqual(dto.Id, dto.ModifiedById);
    }

    #endregion

    #region Nullability Tests

    [Fact]
    public void ModifiedById_SupportsNullablePattern()
    {
        // Arrange
        var dto = new TestAuditableDto();

        // Act - Set to null
        dto.ModifiedById = null;
        var isNull1 = dto.ModifiedById == null;
        var hasValue1 = dto.ModifiedById.HasValue;

        // Set to value
        dto.ModifiedById = 5;
        var isNull2 = dto.ModifiedById == null;
        var hasValue2 = dto.ModifiedById.HasValue;

        // Assert
        Assert.True(isNull1);
        Assert.False(hasValue1);
        Assert.False(isNull2);
        Assert.True(hasValue2);
        Assert.Equal(5, dto.ModifiedById.Value);
    }

    [Fact]
    public void ModifiedOn_SupportsNullablePattern()
    {
        // Arrange
        var dto = new TestAuditableDto();
        var testDate = DateTime.UtcNow;

        // Act - Set to null
        dto.ModifiedOn = null;
        var isNull1 = dto.ModifiedOn == null;
        var hasValue1 = dto.ModifiedOn.HasValue;

        // Set to value
        dto.ModifiedOn = testDate;
        var isNull2 = dto.ModifiedOn == null;
        var hasValue2 = dto.ModifiedOn.HasValue;

        // Assert
        Assert.True(isNull1);
        Assert.False(hasValue1);
        Assert.False(isNull2);
        Assert.True(hasValue2);
        Assert.Equal(testDate, dto.ModifiedOn.Value);
    }

    #endregion

    #region Class Characteristics Tests

    [Fact]
    public void AuditableEntityDto_IsAbstract()
    {
        // Arrange
        var dtoType = typeof(AuditableEntityDto<int>);

        // Act
        var isAbstract = dtoType.IsAbstract;

        // Assert
        Assert.True(isAbstract, "AuditableEntityDto should be abstract");
    }

    [Fact]
    public void AuditableEntityDto_IsInCorrectNamespace()
    {
        // Arrange
        var dtoType = typeof(AuditableEntityDto<>);

        // Act
        var namespaceName = dtoType.Namespace;

        // Assert
        Assert.Equal("JumpStart.Api.DTOs.Advanced", namespaceName);
    }

    [Fact]
    public void AuditableEntityDto_HasPublicProperties()
    {
        // Arrange
        var dtoType = typeof(AuditableEntityDto<int>);

        // Act
        var createdByIdProp = dtoType.GetProperty(nameof(AuditableEntityDto<int>.CreatedById));
        var createdOnProp = dtoType.GetProperty(nameof(AuditableEntityDto<int>.CreatedOn));
        var modifiedByIdProp = dtoType.GetProperty(nameof(AuditableEntityDto<int>.ModifiedById));
        var modifiedOnProp = dtoType.GetProperty(nameof(AuditableEntityDto<int>.ModifiedOn));

        // Assert
        Assert.NotNull(createdByIdProp);
        Assert.True(createdByIdProp!.CanRead && createdByIdProp.CanWrite);
        
        Assert.NotNull(createdOnProp);
        Assert.True(createdOnProp!.CanRead && createdOnProp.CanWrite);
        
        Assert.NotNull(modifiedByIdProp);
        Assert.True(modifiedByIdProp!.CanRead && modifiedByIdProp.CanWrite);
        
        Assert.NotNull(modifiedOnProp);
        Assert.True(modifiedOnProp!.CanRead && modifiedOnProp.CanWrite);
    }

    #endregion
}
