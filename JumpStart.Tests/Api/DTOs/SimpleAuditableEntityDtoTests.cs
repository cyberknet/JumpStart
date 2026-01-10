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
using Xunit;

namespace JumpStart.Tests.Api.DTOs;

/// <summary>
/// Unit tests for the <see cref="SimpleAuditableEntityDto"/> class.
/// Tests property functionality, inheritance, audit field handling, and usage with Guid identifiers.
/// </summary>
public class SimpleAuditableEntityDtoTests
{
    #region Test DTOs

    /// <summary>
    /// Concrete test DTO for testing.
    /// </summary>
    public class TestSimpleAuditableDto : SimpleAuditableEntityDto
    {
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }

    /// <summary>
    /// Another test DTO for polymorphism tests.
    /// </summary>
    public class TestProductDto : SimpleAuditableEntityDto
    {
        public string ProductName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
    }

    #endregion

    #region Property Tests

    [Fact]
    public void Properties_CanBeSet_AndRetrieved()
    {
        // Arrange
        var dto = new TestSimpleAuditableDto();
        var id = Guid.NewGuid();
        var createdById = Guid.NewGuid();
        var modifiedById = Guid.NewGuid();
        var createdOn = DateTime.UtcNow;
        var modifiedOn = DateTime.UtcNow.AddHours(1);

        // Act
        dto.Id = id;
        dto.Name = "Test";
        dto.Price = 99.99m;
        dto.CreatedById = createdById;
        dto.CreatedOn = createdOn;
        dto.ModifiedById = modifiedById;
        dto.ModifiedOn = modifiedOn;

        // Assert
        Assert.Equal(id, dto.Id);
        Assert.Equal("Test", dto.Name);
        Assert.Equal(99.99m, dto.Price);
        Assert.Equal(createdById, dto.CreatedById);
        Assert.Equal(createdOn, dto.CreatedOn);
        Assert.Equal(modifiedById, dto.ModifiedById);
        Assert.Equal(modifiedOn, dto.ModifiedOn);
    }

    [Fact]
    public void CreatedById_StoresGuidValue()
    {
        // Arrange
        var dto = new TestSimpleAuditableDto();
        var userId = Guid.NewGuid();

        // Act
        dto.CreatedById = userId;

        // Assert
        Assert.Equal(userId, dto.CreatedById);
        Assert.IsType<Guid>(dto.CreatedById);
    }

    [Fact]
    public void CreatedOn_StoresUtcDateTime()
    {
        // Arrange
        var dto = new TestSimpleAuditableDto();
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
        var dto = new TestSimpleAuditableDto
        {
            CreatedById = Guid.NewGuid(),
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
        var dto = new TestSimpleAuditableDto();
        var userId = Guid.NewGuid();

        // Act
        dto.ModifiedById = userId;

        // Assert
        Assert.NotNull(dto.ModifiedById);
        Assert.Equal(userId, dto.ModifiedById.Value);
    }

    [Fact]
    public void ModifiedOn_CanBeNull()
    {
        // Arrange
        var dto = new TestSimpleAuditableDto
        {
            CreatedById = Guid.NewGuid(),
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
        var dto = new TestSimpleAuditableDto();
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
    public void SimpleAuditableEntityDto_InheritsFrom_SimpleEntityDto()
    {
        // Arrange
        var dtoType = typeof(SimpleAuditableEntityDto);

        // Act
        var baseType = dtoType.BaseType;

        // Assert
        Assert.NotNull(baseType);
        Assert.Equal(typeof(SimpleEntityDto), baseType);
    }

    [Fact]
    public void SimpleAuditableEntityDto_HasIdProperty_FromBase()
    {
        // Arrange
        var dto = new TestSimpleAuditableDto();
        var id = Guid.NewGuid();

        // Act
        dto.Id = id;

        // Assert
        Assert.Equal(id, dto.Id);
    }

    [Fact]
    public void SimpleAuditableEntityDto_ImplementsIDto_ThroughInheritance()
    {
        // Arrange
        var dto = new TestSimpleAuditableDto();

        // Act
        var isDto = dto is IDto;

        // Assert
        Assert.True(isDto);
    }

    [Fact]
    public void SimpleAuditableEntityDto_HasCorrectPropertyCount()
    {
        // Arrange
        var dtoType = typeof(SimpleAuditableEntityDto);

        // Act - Get public instance properties declared in SimpleAuditableEntityDto
        var properties = dtoType.GetProperties().Where(p => p.DeclaringType == typeof(SimpleAuditableEntityDto)).ToList();

        // Assert - Should have 4 audit properties
        Assert.Equal(4, properties.Count);
        Assert.Contains(properties, p => p.Name == nameof(SimpleAuditableEntityDto.CreatedById));
        Assert.Contains(properties, p => p.Name == nameof(SimpleAuditableEntityDto.CreatedOn));
        Assert.Contains(properties, p => p.Name == nameof(SimpleAuditableEntityDto.ModifiedById));
        Assert.Contains(properties, p => p.Name == nameof(SimpleAuditableEntityDto.ModifiedOn));
    }

    #endregion

    #region Audit Scenario Tests

    [Fact]
    public void NewEntity_HasCreationAudit_NoModificationAudit()
    {
        // Arrange
        var createdDate = new DateTime(2026, 1, 15, 10, 30, 0, DateTimeKind.Utc);
        var creatorId = Guid.NewGuid();

        // Act - Simulate a newly created entity
        var dto = new TestSimpleAuditableDto
        {
            Id = Guid.NewGuid(),
            Name = "New Entity",
            Price = 50.00m,
            CreatedById = creatorId,
            CreatedOn = createdDate,
            ModifiedById = null,
            ModifiedOn = null
        };

        // Assert
        Assert.Equal(creatorId, dto.CreatedById);
        Assert.Equal(createdDate, dto.CreatedOn);
        Assert.Null(dto.ModifiedById);
        Assert.Null(dto.ModifiedOn);
    }

    [Fact]
    public void ModifiedEntity_HasBothCreationAndModificationAudit()
    {
        // Arrange
        var createdDate = new DateTime(2026, 1, 15, 10, 30, 0, DateTimeKind.Utc);
        var modifiedDate = new DateTime(2026, 1, 20, 14, 45, 0, DateTimeKind.Utc);
        var creatorId = Guid.NewGuid();
        var modifierId = Guid.NewGuid();

        // Act - Simulate a modified entity
        var dto = new TestSimpleAuditableDto
        {
            Id = Guid.NewGuid(),
            Name = "Modified Entity",
            Price = 75.00m,
            CreatedById = creatorId,
            CreatedOn = createdDate,
            ModifiedById = modifierId,
            ModifiedOn = modifiedDate
        };

        // Assert
        Assert.Equal(creatorId, dto.CreatedById);
        Assert.Equal(createdDate, dto.CreatedOn);
        Assert.Equal(modifierId, dto.ModifiedById);
        Assert.Equal(modifiedDate, dto.ModifiedOn);
        Assert.True(dto.ModifiedOn > dto.CreatedOn);
    }

    [Fact]
    public void SelfModifiedEntity_CanHaveSameCreatorAndModifier()
    {
        // Arrange
        var createdDate = new DateTime(2026, 1, 15, 10, 30, 0, DateTimeKind.Utc);
        var modifiedDate = new DateTime(2026, 1, 15, 11, 00, 0, DateTimeKind.Utc);
        var userId = Guid.NewGuid();

        // Act - User creates and modifies their own entity
        var dto = new TestSimpleAuditableDto
        {
            Id = Guid.NewGuid(),
            Name = "Self-Modified Entity",
            Price = 100.00m,
            CreatedById = userId,
            CreatedOn = createdDate,
            ModifiedById = userId,
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
        var entityId = Guid.NewGuid();
        var creatorId = Guid.NewGuid();
        var modifierId = Guid.NewGuid();

        var dto = new TestSimpleAuditableDto
        {
            Id = entityId,
            Name = "Product",
            Price = 50.00m,
            CreatedById = creatorId,
            CreatedOn = DateTime.UtcNow,
            ModifiedById = modifierId,
            ModifiedOn = DateTime.UtcNow.AddDays(1)
        };

        // Assert - Audit fields don't affect entity data
        Assert.NotEqual(dto.Id, dto.CreatedById);
        Assert.NotEqual(dto.Id, dto.ModifiedById);
    }

    #endregion

    #region Guid Identifier Tests

    [Fact]
    public void SimpleAuditableEntityDto_UsesGuid_ForId()
    {
        // Arrange
        var dto = new TestSimpleAuditableDto();
        var id = Guid.NewGuid();

        // Act
        dto.Id = id;

        // Assert
        Assert.IsType<Guid>(dto.Id);
        Assert.Equal(id, dto.Id);
    }

    [Fact]
    public void SimpleAuditableEntityDto_UsesGuid_ForAuditIds()
    {
        // Arrange
        var dto = new TestSimpleAuditableDto();
        var creatorId = Guid.NewGuid();
        var modifierId = Guid.NewGuid();

        // Act
        dto.CreatedById = creatorId;
        dto.ModifiedById = modifierId;

        // Assert
        Assert.IsType<Guid>(dto.CreatedById);
        Assert.IsType<Guid>(dto.ModifiedById.Value);
    }

    [Fact]
    public void SimpleAuditableEntityDto_AllGuids_AreUnique()
    {
        // Arrange
        var dto = new TestSimpleAuditableDto
        {
            Id = Guid.NewGuid(),
            CreatedById = Guid.NewGuid(),
            CreatedOn = DateTime.UtcNow,
            ModifiedById = Guid.NewGuid(),
            ModifiedOn = DateTime.UtcNow.AddHours(1)
        };

        // Act
        var allGuids = new[] { dto.Id, dto.CreatedById, dto.ModifiedById!.Value };

        // Assert
        Assert.Equal(allGuids.Length, allGuids.Distinct().Count());
    }

    #endregion

    #region Nullability Tests

    [Fact]
    public void ModifiedById_SupportsNullablePattern()
    {
        // Arrange
        var dto = new TestSimpleAuditableDto();
        var userId = Guid.NewGuid();

        // Act - Set to null
        dto.ModifiedById = null;
        var isNull1 = dto.ModifiedById == null;
        var hasValue1 = dto.ModifiedById.HasValue;

        // Set to value
        dto.ModifiedById = userId;
        var isNull2 = dto.ModifiedById == null;
        var hasValue2 = dto.ModifiedById.HasValue;

        // Assert
        Assert.True(isNull1);
        Assert.False(hasValue1);
        Assert.False(isNull2);
        Assert.True(hasValue2);
        Assert.Equal(userId, dto.ModifiedById.Value);
    }

    [Fact]
    public void ModifiedOn_SupportsNullablePattern()
    {
        // Arrange
        var dto = new TestSimpleAuditableDto();
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
    public void SimpleAuditableEntityDto_IsAbstract()
    {
        // Arrange
        var dtoType = typeof(SimpleAuditableEntityDto);

        // Act
        var isAbstract = dtoType.IsAbstract;

        // Assert
        Assert.True(isAbstract, "SimpleAuditableEntityDto should be abstract");
    }

    [Fact]
    public void SimpleAuditableEntityDto_IsInCorrectNamespace()
    {
        // Arrange
        var dtoType = typeof(SimpleAuditableEntityDto);

        // Act
        var namespaceName = dtoType.Namespace;

        // Assert
        Assert.Equal("JumpStart.Api.DTOs", namespaceName);
    }

    [Fact]
    public void SimpleAuditableEntityDto_HasPublicProperties()
    {
        // Arrange
        var dtoType = typeof(SimpleAuditableEntityDto);

        // Act
        var createdByIdProp = dtoType.GetProperty(nameof(SimpleAuditableEntityDto.CreatedById));
        var createdOnProp = dtoType.GetProperty(nameof(SimpleAuditableEntityDto.CreatedOn));
        var modifiedByIdProp = dtoType.GetProperty(nameof(SimpleAuditableEntityDto.ModifiedById));
        var modifiedOnProp = dtoType.GetProperty(nameof(SimpleAuditableEntityDto.ModifiedOn));

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

    #region Usage Scenario Tests

    [Fact]
    public void SimpleAuditableEntityDto_CanRepresent_TrackedEntity()
    {
        // Arrange & Act - Simulating entity from API response
        var dto = new TestProductDto
        {
            Id = Guid.NewGuid(),
            ProductName = "Laptop",
            Category = "Electronics",
            CreatedById = Guid.NewGuid(),
            CreatedOn = new DateTime(2026, 1, 15, 10, 30, 0, DateTimeKind.Utc),
            ModifiedById = Guid.NewGuid(),
            ModifiedOn = new DateTime(2026, 1, 20, 14, 45, 0, DateTimeKind.Utc)
        };

        // Assert
        Assert.NotEqual(Guid.Empty, dto.Id);
        Assert.Equal("Laptop", dto.ProductName);
        Assert.NotEqual(Guid.Empty, dto.CreatedById);
        Assert.NotEqual(default, dto.CreatedOn);
        Assert.NotNull(dto.ModifiedById);
        Assert.NotNull(dto.ModifiedOn);
    }

    [Fact]
    public void SimpleAuditableEntityDto_SupportsObjectInitializer()
    {
        // Arrange & Act
        var dto = new TestSimpleAuditableDto
        {
            Id = Guid.NewGuid(),
            Name = "Initialized",
            Price = 100.00m,
            CreatedById = Guid.NewGuid(),
            CreatedOn = DateTime.UtcNow
        };

        // Assert
        Assert.NotEqual(Guid.Empty, dto.Id);
        Assert.Equal("Initialized", dto.Name);
        Assert.Equal(100.00m, dto.Price);
        Assert.NotEqual(Guid.Empty, dto.CreatedById);
    }

    [Fact]
    public void SimpleAuditableEntityDto_CanShowAuditTrail()
    {
        // Arrange
        var createdDate = new DateTime(2026, 1, 15, 10, 30, 0, DateTimeKind.Utc);
        var modifiedDate = new DateTime(2026, 1, 20, 14, 45, 0, DateTimeKind.Utc);
        var creatorId = Guid.Parse("1a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c6d");
        var modifierId = Guid.Parse("9f8e7d6c-5b4a-3210-fedc-ba9876543210");

        var dto = new TestSimpleAuditableDto
        {
            Id = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
            Name = "Product",
            Price = 999.99m,
            CreatedById = creatorId,
            CreatedOn = createdDate,
            ModifiedById = modifierId,
            ModifiedOn = modifiedDate
        };

        // Act
        var auditTrail = $"Created by {dto.CreatedById} on {dto.CreatedOn:yyyy-MM-dd HH:mm:ss}";
        if (dto.ModifiedOn.HasValue)
        {
            auditTrail += $", Modified by {dto.ModifiedById} on {dto.ModifiedOn:yyyy-MM-dd HH:mm:ss}";
        }

        // Assert
        Assert.Contains(creatorId.ToString(), auditTrail);
        Assert.Contains(modifierId.ToString(), auditTrail);
        Assert.Contains("2026-01-15", auditTrail);
        Assert.Contains("2026-01-20", auditTrail);
    }

    #endregion
}
