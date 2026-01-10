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
/// Unit tests for the <see cref="IUpdateDto{TKey}"/> interface.
/// Tests interface implementation, Id property, type constraints, and proper usage for update operations.
/// </summary>
public class IUpdateDtoTests
{
    #region Test DTOs

    /// <summary>
    /// Simple update DTO with int key.
    /// </summary>
    public class UpdateTestProductDto : IUpdateDto<int>
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }

    /// <summary>
    /// Update DTO with long key.
    /// </summary>
    public class UpdateTestOrderDto : IUpdateDto<long>
    {
        public long Id { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
    }

    /// <summary>
    /// Update DTO with Guid key.
    /// </summary>
    public class UpdateTestCustomerDto : IUpdateDto<Guid>
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    /// <summary>
    /// Nested update DTO for testing composition.
    /// </summary>
    public class UpdateTestOrderItemDto : IUpdateDto<int>
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

    /// <summary>
    /// Update DTO with nested items.
    /// </summary>
    public class UpdateTestComplexDto : IUpdateDto<int>
    {
        public int Id { get; set; }
        public List<UpdateTestOrderItemDto> Items { get; set; } = new();
    }

    #endregion

    #region Interface Inheritance Tests

    [Fact]
    public void IUpdateDto_InheritsFrom_IDto()
    {
        // Arrange
        var interfaceType = typeof(IUpdateDto<int>);

        // Act
        var inheritsFromIDto = typeof(IDto).IsAssignableFrom(interfaceType);

        // Assert
        Assert.True(inheritsFromIDto, "IUpdateDto should inherit from IDto");
    }

    [Fact]
    public void IUpdateDto_IsInterface()
    {
        // Arrange
        var interfaceType = typeof(IUpdateDto<>);

        // Act
        var isInterface = interfaceType.IsInterface;

        // Assert
        Assert.True(isInterface, "IUpdateDto should be an interface");
    }

    [Fact]
    public void IUpdateDto_IsInCorrectNamespace()
    {
        // Arrange
        var interfaceType = typeof(IUpdateDto<>);

        // Act
        var namespaceName = interfaceType.Namespace;

        // Assert
        Assert.Equal("JumpStart.Api.DTOs", namespaceName);
    }

    #endregion

    #region Generic Type Parameter Tests

    [Fact]
    public void IUpdateDto_HasOneGenericParameter()
    {
        // Arrange
        var interfaceType = typeof(IUpdateDto<>);

        // Act
        var genericParameters = interfaceType.GetGenericArguments();

        // Assert
        Assert.Single(genericParameters);
        Assert.Equal("TKey", genericParameters[0].Name);
    }

    [Fact]
    public void IUpdateDto_GenericParameter_HasStructConstraint()
    {
        // Arrange
        var interfaceType = typeof(IUpdateDto<>);
        var genericParameter = interfaceType.GetGenericArguments()[0];

        // Act
        var hasValueTypeConstraint = (genericParameter.GenericParameterAttributes & System.Reflection.GenericParameterAttributes.NotNullableValueTypeConstraint) != 0;

        // Assert
        Assert.True(hasValueTypeConstraint, "TKey should have struct constraint");
    }

    [Fact]
    public void IUpdateDto_WorksWithIntKey()
    {
        // Arrange & Act
        var dto = new UpdateTestProductDto
        {
            Id = 123,
            Name = "Test Product",
            Price = 99.99m
        };

        // Assert
        Assert.Equal(123, dto.Id);
        Assert.IsType<int>(dto.Id);
    }

    [Fact]
    public void IUpdateDto_WorksWithLongKey()
    {
        // Arrange & Act
        var dto = new UpdateTestOrderDto
        {
            Id = 9876543210L,
            OrderDate = DateTime.UtcNow,
            TotalAmount = 499.99m
        };

        // Assert
        Assert.Equal(9876543210L, dto.Id);
        Assert.IsType<long>(dto.Id);
    }

    [Fact]
    public void IUpdateDto_WorksWithGuidKey()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var dto = new UpdateTestCustomerDto
        {
            Id = guid,
            Name = "Test Customer",
            Email = "test@example.com"
        };

        // Assert
        Assert.Equal(guid, dto.Id);
        Assert.IsType<Guid>(dto.Id);
    }

    #endregion

    #region Id Property Tests

    [Fact]
    public void IUpdateDto_HasIdProperty()
    {
        // Arrange
        var interfaceType = typeof(IUpdateDto<int>);

        // Act
        var idProperty = interfaceType.GetProperty("Id");

        // Assert
        Assert.NotNull(idProperty);
        Assert.Equal(typeof(int), idProperty!.PropertyType);
    }

    [Fact]
    public void IUpdateDto_IdProperty_IsReadWrite()
    {
        // Arrange
        var interfaceType = typeof(IUpdateDto<int>);
        var idProperty = interfaceType.GetProperty("Id");

        // Act
        var canRead = idProperty!.CanRead;
        var canWrite = idProperty.CanWrite;

        // Assert
        Assert.True(canRead, "Id property should be readable");
        Assert.True(canWrite, "Id property should be writable");
    }

    [Fact]
    public void UpdateDto_IdCanBeSet_AndRetrieved()
    {
        // Arrange
        var dto = new UpdateTestProductDto();

        // Act
        dto.Id = 42;
        var retrievedId = dto.Id;

        // Assert
        Assert.Equal(42, retrievedId);
    }

    [Fact]
    public void UpdateDto_IdType_MatchesGenericParameter()
    {
        // Arrange
        var intDto = new UpdateTestProductDto();
        var longDto = new UpdateTestOrderDto();
        var guidDto = new UpdateTestCustomerDto();

        // Act
        intDto.Id = 1;
        longDto.Id = 2L;
        guidDto.Id = Guid.NewGuid();

        // Assert
        Assert.IsType<int>(intDto.Id);
        Assert.IsType<long>(longDto.Id);
        Assert.IsType<Guid>(guidDto.Id);
    }

    #endregion

    #region Implementation Tests

    [Fact]
    public void UpdateDto_Implementation_CanSetProperties()
    {
        // Arrange
        var dto = new UpdateTestProductDto();

        // Act
        dto.Id = 100;
        dto.Name = "Updated Product";
        dto.Price = 149.99m;

        // Assert
        Assert.Equal(100, dto.Id);
        Assert.Equal("Updated Product", dto.Name);
        Assert.Equal(149.99m, dto.Price);
    }

    [Fact]
    public void UpdateDto_CanBeAssignedTo_IDto()
    {
        // Arrange
        var updateDto = new UpdateTestProductDto { Id = 1, Name = "Product", Price = 50.00m };

        // Act
        IDto dto = updateDto;

        // Assert
        Assert.NotNull(dto);
        Assert.IsAssignableFrom<IDto>(updateDto);
        Assert.IsAssignableFrom<IUpdateDto<int>>(updateDto);
    }

    [Fact]
    public void UpdateDto_CanBeAssignedTo_IUpdateDto()
    {
        // Arrange
        var dto = new UpdateTestProductDto { Id = 1, Name = "Product", Price = 50.00m };

        // Act
        IUpdateDto<int> updateDto = dto;

        // Assert
        Assert.NotNull(updateDto);
        Assert.Equal(1, updateDto.Id);
    }

    #endregion

    #region Nested Update DTO Tests

    [Fact]
    public void UpdateDto_CanContain_NestedUpdateDtos()
    {
        // Arrange
        var dto = new UpdateTestComplexDto
        {
            Id = 1,
            Items = new List<UpdateTestOrderItemDto>
            {
                new UpdateTestOrderItemDto { Id = 10, ProductId = 1, Quantity = 2 },
                new UpdateTestOrderItemDto { Id = 20, ProductId = 2, Quantity = 1 }
            }
        };

        // Act
        var itemsAreUpdateDtos = dto.Items.All(item => item is IUpdateDto<int>);

        // Assert
        Assert.True(itemsAreUpdateDtos);
        Assert.Equal(2, dto.Items.Count);
        Assert.All(dto.Items, item => Assert.NotEqual(0, item.Id));
    }

    [Fact]
    public void NestedUpdateDto_IsAlso_IUpdateDto()
    {
        // Arrange
        var nestedDto = new UpdateTestOrderItemDto { Id = 5, ProductId = 1, Quantity = 10 };

        // Act
        var isUpdateDto = nestedDto is IUpdateDto<int>;
        var isDto = nestedDto is IDto;

        // Assert
        Assert.True(isUpdateDto);
        Assert.True(isDto);
    }

    #endregion

    #region Type Identification Tests

    [Fact]
    public void IUpdateDto_CanBeIdentified_AtRuntime()
    {
        // Arrange
        object obj = new UpdateTestProductDto { Id = 1, Name = "Test", Price = 10.00m };

        // Act
        var isUpdateDto = obj is IUpdateDto<int>;

        // Assert
        Assert.True(isUpdateDto);
    }

    [Fact]
    public void IUpdateDto_CanBeUsedIn_Collections()
    {
        // Arrange
        var updateDtos = new List<IUpdateDto<int>>
        {
            new UpdateTestProductDto { Id = 1, Name = "Product1", Price = 10.00m },
            new UpdateTestOrderItemDto { Id = 2, ProductId = 1, Quantity = 5 }
        };

        // Act
        var count = updateDtos.Count;

        // Assert
        Assert.Equal(2, count);
        Assert.All(updateDtos, dto => Assert.IsAssignableFrom<IUpdateDto<int>>(dto));
        Assert.All(updateDtos, dto => Assert.NotEqual(0, dto.Id));
    }

    [Fact]
    public void IUpdateDto_CanBeFiltered_FromMixedDtos()
    {
        // Arrange
        var dtos = new List<IDto>
        {
            new UpdateTestProductDto { Id = 1, Name = "Product", Price = 10.00m }
        };

        // Act
        var updateDtos = dtos.OfType<IUpdateDto<int>>().ToList();

        // Assert
        Assert.Single(updateDtos);
        Assert.All(updateDtos, dto => Assert.IsAssignableFrom<IUpdateDto<int>>(dto));
    }

    #endregion

    #region Property Validation Tests

    [Fact]
    public void UpdateDto_Must_HaveIdProperty()
    {
        // Arrange
        var dtoType = typeof(UpdateTestProductDto);

        // Act
        var hasIdProperty = dtoType.GetProperty("Id") != null;

        // Assert
        Assert.True(hasIdProperty, "Update DTOs must have Id property (required by IUpdateDto)");
    }

    [Fact]
    public void UpdateDto_ShouldNot_HaveAuditProperties()
    {
        // Arrange
        var dtoType = typeof(UpdateTestProductDto);

        // Act
        var hasCreatedBy = dtoType.GetProperty("CreatedById") != null;
        var hasCreatedOn = dtoType.GetProperty("CreatedOn") != null;
        var hasModifiedBy = dtoType.GetProperty("ModifiedById") != null;
        var hasModifiedOn = dtoType.GetProperty("ModifiedOn") != null;

        // Assert
        Assert.False(hasCreatedBy, "Update DTOs should not have CreatedById");
        Assert.False(hasCreatedOn, "Update DTOs should not have CreatedOn");
        Assert.False(hasModifiedBy, "Update DTOs should not have ModifiedById");
        Assert.False(hasModifiedOn, "Update DTOs should not have ModifiedOn");
    }

    [Fact]
    public void UpdateDto_Should_HaveBusinessProperties()
    {
        // Arrange
        var dtoType = typeof(UpdateTestProductDto);

        // Act
        var hasName = dtoType.GetProperty("Name") != null;
        var hasPrice = dtoType.GetProperty("Price") != null;

        // Assert
        Assert.True(hasName, "Update DTOs should have business properties");
        Assert.True(hasPrice, "Update DTOs should have business properties");
    }

    #endregion

    #region Generic Constraint Tests

    [Fact]
    public void IUpdateDto_CanBeUsed_AsGenericConstraint()
    {
        // Arrange
        var dto = new UpdateTestProductDto { Id = 1, Name = "Test", Price = 10.00m };

        // Act
        var result = ProcessUpdateDto(dto);

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public void GenericMethod_CanAccess_IdProperty()
    {
        // Arrange
        var intDto = new UpdateTestProductDto { Id = 42, Name = "Test", Price = 10.00m };
        var guidDto = new UpdateTestCustomerDto { Id = Guid.NewGuid(), Name = "Test", Email = "test@test.com" };

        // Act
        var intId = GetIdInt(intDto);
        var guidId = GetIdGuid(guidDto);

        // Assert
        Assert.Equal(42, intId);
        Assert.NotEqual(Guid.Empty, guidId);
    }

    // Helper methods demonstrating generic usage
    private int ProcessUpdateDto<T>(T dto) where T : IUpdateDto<int>
    {
        return dto.Id;
    }

    private int GetIdInt<TDto>(TDto dto) where TDto : IUpdateDto<int>
    {
        return dto.Id;
    }

    private Guid GetIdGuid<TDto>(TDto dto) where TDto : IUpdateDto<Guid>
    {
        return dto.Id;
    }

    #endregion

    #region Usage Scenario Tests

    [Fact]
    public void UpdateDto_CanRepresent_ModifiedEntityData()
    {
        // Arrange & Act - Simulating data from client for entity update
        var dto = new UpdateTestProductDto
        {
            Id = 123, // Required to identify which entity to update
            Name = "Updated Product Name",
            Price = 79.99m
        };

        // Assert
        Assert.Equal(123, dto.Id);
        Assert.Equal("Updated Product Name", dto.Name);
        Assert.Equal(79.99m, dto.Price);
    }

    [Fact]
    public void UpdateDto_SupportsObjectInitializer()
    {
        // Arrange & Act
        var dto = new UpdateTestProductDto
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
    public void UpdateDto_IdMismatch_CanBeValidated()
    {
        // Arrange
        var urlId = 1;
        var dto = new UpdateTestProductDto
        {
            Id = 2, // Different from URL
            Name = "Product",
            Price = 50.00m
        };

        // Act
        var idsMatch = urlId == dto.Id;

        // Assert
        Assert.False(idsMatch, "Should detect ID mismatch between URL and DTO");
    }

    [Fact]
    public void UpdateDto_IdMatch_CanBeValidated()
    {
        // Arrange
        var urlId = 123;
        var dto = new UpdateTestProductDto
        {
            Id = 123, // Matches URL
            Name = "Product",
            Price = 50.00m
        };

        // Act
        var idsMatch = urlId == dto.Id;

        // Assert
        Assert.True(idsMatch, "Should confirm ID match between URL and DTO");
    }

    #endregion

    #region Comparison with ICreateDto Tests

    [Fact]
    public void IUpdateDto_DiffersFrom_ICreateDto()
    {
        // Arrange
        var updateDtoType = typeof(IUpdateDto<int>);
        var createDtoType = typeof(ICreateDto);

        // Act
        var updateHasId = updateDtoType.GetProperty("Id") != null;
        var createHasId = createDtoType.GetProperty("Id") != null;

        // Assert
        Assert.True(updateHasId, "IUpdateDto should have Id property");
        Assert.False(createHasId, "ICreateDto should not have Id property");
    }

    [Fact]
    public void UpdateDto_RequiresId_CreateDtoDoesNot()
    {
        // Arrange - This test demonstrates the key difference

        // Update DTO requires Id
        var updateDto = new UpdateTestProductDto
        {
            Id = 1, // Required
            Name = "Product",
            Price = 10.00m
        };

        // Assert
        Assert.NotEqual(0, updateDto.Id);
    }

    #endregion
}
