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
using JumpStart.Api.DTOs.Advanced;
using Xunit;

namespace JumpStart.Tests.Api.DTOs;

/// <summary>
/// Unit tests for the <see cref="IDto"/> interface.
/// Tests marker interface pattern, type identification, hierarchy, and usage in generic contexts.
/// </summary>
public class IDtoTests
{
    #region Test DTOs

    /// <summary>
    /// Simple test DTO implementing IDto directly.
    /// </summary>
    public class DirectTestDto : IDto
    {
        public string Name { get; set; } = string.Empty;
    }

    /// <summary>
    /// Test DTO implementing IDto through ICreateDto.
    /// </summary>
    public class CreateTestDto : ICreateDto
    {
        public string Title { get; set; } = string.Empty;
    }

    /// <summary>
    /// Test DTO implementing IDto through EntityDto.
    /// </summary>
    public class EntityTestDto : EntityDto<int>
    {
        public string Description { get; set; } = string.Empty;
    }

    /// <summary>
    /// Another test DTO for polymorphism tests.
    /// </summary>
    public class AnotherTestDto : IDto
    {
        public decimal Value { get; set; }
    }

    #endregion

    #region Interface Characteristics Tests

    [Fact]
    public void IDto_IsInterface()
    {
        // Arrange
        var interfaceType = typeof(IDto);

        // Act
        var isInterface = interfaceType.IsInterface;

        // Assert
        Assert.True(isInterface, "IDto should be an interface");
    }

    [Fact]
    public void IDto_IsPublic()
    {
        // Arrange
        var interfaceType = typeof(IDto);

        // Act
        var isPublic = interfaceType.IsPublic;

        // Assert
        Assert.True(isPublic, "IDto should be public");
    }

    [Fact]
    public void IDto_IsInCorrectNamespace()
    {
        // Arrange
        var interfaceType = typeof(IDto);

        // Act
        var namespaceName = interfaceType.Namespace;

        // Assert
        Assert.Equal("JumpStart.Api.DTOs", namespaceName);
    }

    [Fact]
    public void IDto_HasNoBaseInterfaces()
    {
        // Arrange
        var interfaceType = typeof(IDto);

        // Act
        var baseInterfaces = interfaceType.GetInterfaces();

        // Assert
        Assert.Empty(baseInterfaces); // Root interface has no base interfaces
    }

    #endregion

    #region Marker Interface Pattern Tests

    [Fact]
    public void IDto_HasNoMembers()
    {
        // Arrange
        var interfaceType = typeof(IDto);

        // Act
        var declaredMembers = interfaceType.GetMembers(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly);

        // Assert
        Assert.Empty(declaredMembers); // Marker interface has no members
    }

    [Fact]
    public void IDto_HasNoProperties()
    {
        // Arrange
        var interfaceType = typeof(IDto);

        // Act
        var properties = interfaceType.GetProperties();

        // Assert
        Assert.Empty(properties);
    }

    [Fact]
    public void IDto_HasNoMethods()
    {
        // Arrange
        var interfaceType = typeof(IDto);

        // Act
        var methods = interfaceType.GetMethods().Where(m => !m.IsSpecialName).ToList(); // Exclude property accessors

        // Assert
        Assert.Empty(methods);
    }

    [Fact]
    public void IDto_FollowsMarkerPattern()
    {
        // Arrange
        var interfaceType = typeof(IDto);

        // Act
        var hasAnyMembers = interfaceType.GetMembers(System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).Any();

        // Assert
        Assert.False(hasAnyMembers, "Marker interface should not have any members");
    }

    #endregion

    #region Direct Implementation Tests

    [Fact]
    public void DirectDto_Implementation_IsAssignableToIDto()
    {
        // Arrange
        var dto = new DirectTestDto { Name = "Test" };

        // Act
        var isDto = dto is IDto;

        // Assert
        Assert.True(isDto);
    }

    [Fact]
    public void DirectDto_CanBeAssignedTo_IDto()
    {
        // Arrange
        var dto = new DirectTestDto { Name = "Test" };

        // Act
        IDto idto = dto;

        // Assert
        Assert.NotNull(idto);
        Assert.IsAssignableFrom<IDto>(dto);
    }

    [Fact]
    public void DirectDto_CanBeIdentified_AtRuntime()
    {
        // Arrange
        object obj = new DirectTestDto { Name = "Test" };

        // Act
        var isDto = obj is IDto;

        // Assert
        Assert.True(isDto);
    }

    #endregion

    #region Inheritance Hierarchy Tests

    [Fact]
    public void ICreateDto_InheritsFrom_IDto()
    {
        // Arrange
        var createDtoType = typeof(ICreateDto);

        // Act
        var inheritsFromIDto = typeof(IDto).IsAssignableFrom(createDtoType);

        // Assert
        Assert.True(inheritsFromIDto, "ICreateDto should inherit from IDto");
    }

    [Fact]
    public void EntityDto_InheritsFrom_IDto()
    {
        // Arrange
        var entityDtoType = typeof(EntityDto<int>);

        // Act
        var inheritsFromIDto = typeof(IDto).IsAssignableFrom(entityDtoType);

        // Assert
        Assert.True(inheritsFromIDto, "EntityDto should inherit from IDto");
    }

    [Fact]
    public void CreateDto_Instance_IsAlsoIDto()
    {
        // Arrange
        var createDto = new CreateTestDto { Title = "Test" };

        // Act
        var isDto = createDto is IDto;
        var isCreateDto = createDto is ICreateDto;

        // Assert
        Assert.True(isDto, "ICreateDto instance should also be IDto");
        Assert.True(isCreateDto);
    }

    [Fact]
    public void EntityDto_Instance_IsAlsoIDto()
    {
        // Arrange
        var entityDto = new EntityTestDto { Id = 1, Description = "Test" };

        // Act
        var isDto = entityDto is IDto;

        // Assert
        Assert.True(isDto, "EntityDto instance should also be IDto");
    }

    #endregion

    #region Generic Constraint Tests

    [Fact]
    public void IDto_CanBeUsed_AsGenericConstraint()
    {
        // Arrange
        var dto = new DirectTestDto { Name = "Test" };

        // Act
        var result = ProcessDto(dto);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void GenericMethod_WorksWith_DifferentDtoTypes()
    {
        // Arrange
        var directDto = new DirectTestDto { Name = "Direct" };
        var createDto = new CreateTestDto { Title = "Create" };
        var entityDto = new EntityTestDto { Id = 1, Description = "Entity" };

        // Act
        var result1 = ProcessDto(directDto);
        var result2 = ProcessDto(createDto);
        var result3 = ProcessDto(entityDto);

        // Assert
        Assert.True(result1);
        Assert.True(result2);
        Assert.True(result3);
    }

    // Helper method demonstrating generic constraint usage
    private bool ProcessDto<T>(T dto) where T : IDto
    {
        return dto != null;
    }

    #endregion

    #region Polymorphic Collection Tests

    [Fact]
    public void IDto_CanBeUsed_InCollections()
    {
        // Arrange
        var dtos = new List<IDto>
        {
            new DirectTestDto { Name = "Test1" },
            new CreateTestDto { Title = "Test2" },
            new EntityTestDto { Id = 1, Description = "Test3" }
        };

        // Act
        var count = dtos.Count;

        // Assert
        Assert.Equal(3, count);
        Assert.All(dtos, dto => Assert.IsAssignableFrom<IDto>(dto));
    }

    [Fact]
    public void IDto_Collection_SupportsPolymorphism()
    {
        // Arrange
        var dtos = new List<IDto>
        {
            new DirectTestDto { Name = "Direct" },
            new AnotherTestDto { Value = 99.99m }
        };

        // Act
        var types = dtos.Select(d => d.GetType().Name).ToList();

        // Assert
        Assert.Contains("DirectTestDto", types);
        Assert.Contains("AnotherTestDto", types);
    }

    [Fact]
    public void IDto_CanBeFiltered_ByType()
    {
        // Arrange
        var dtos = new List<IDto>
        {
            new DirectTestDto { Name = "Test1" },
            new CreateTestDto { Title = "Test2" },
            new DirectTestDto { Name = "Test3" }
        };

        // Act
        var directDtos = dtos.OfType<DirectTestDto>().ToList();

        // Assert
        Assert.Equal(2, directDtos.Count);
    }

    #endregion

    #region Type Identification Tests

    [Fact]
    public void TypeCheck_CanIdentify_Dtos()
    {
        // Arrange
        object dto = new DirectTestDto { Name = "Test" };
        object notDto = "Not a DTO";

        // Act
        var isDtoTrue = dto is IDto;
        var isDtoFalse = notDto is IDto;

        // Assert
        Assert.True(isDtoTrue);
        Assert.False(isDtoFalse);
    }

    [Fact]
    public void TypeCheck_WorksWith_GetType()
    {
        // Arrange
        var dto = new DirectTestDto { Name = "Test" };

        // Act
        var dtoType = dto.GetType();
        var implementsIDto = typeof(IDto).IsAssignableFrom(dtoType);

        // Assert
        Assert.True(implementsIDto);
    }

    [Fact]
    public void IDto_CanBeUsed_InSwitchPattern()
    {
        // Arrange
        IDto dto = new DirectTestDto { Name = "Test" };

        // Act
        var result = dto switch
        {
            DirectTestDto d => $"Direct: {d.Name}",
            CreateTestDto c => $"Create: {c.Title}",
            IDto _ => "Other DTO",
            _ => "Not a DTO"
        };

        // Assert
        Assert.Equal("Direct: Test", result);
    }

    #endregion

    #region Framework Integration Tests

    [Fact]
    public void IDto_EnablesFrameworkLevel_Processing()
    {
        // Arrange
        var processor = new TestDtoProcessor();
        var dtos = new List<IDto>
        {
            new DirectTestDto { Name = "Test1" },
            new CreateTestDto { Title = "Test2" }
        };

        // Act
        var processedCount = processor.ProcessAll(dtos);

        // Assert
        Assert.Equal(2, processedCount);
    }

    [Fact]
    public void IDto_SupportsValidatorPattern()
    {
        // Arrange
        var validator = new TestDtoValidator<DirectTestDto>();
        var dto = new DirectTestDto { Name = "Valid" };

        // Act
        var isValid = validator.Validate(dto);

        // Assert
        Assert.True(isValid);
    }

    // Test helper classes
    private class TestDtoProcessor
    {
        public int ProcessAll(IEnumerable<IDto> dtos)
        {
            return dtos.Count();
        }
    }

    private class TestDtoValidator<T> where T : IDto
    {
        public bool Validate(T dto)
        {
            return dto != null;
        }
    }

    #endregion

    #region Design Pattern Validation Tests

    [Fact]
    public void IDto_ServesAs_RootMarkerInterface()
    {
        // Arrange
        var dtoType = typeof(IDto);

        // Act
        var hasNoMembers = !dtoType.GetMembers(System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Public).Any();
        var hasNoBaseInterfaces = !dtoType.GetInterfaces().Any();

        // Assert
        Assert.True(hasNoMembers, "Root marker interface should have no members");
        Assert.True(hasNoBaseInterfaces, "Root marker interface should have no base interfaces");
    }

    [Fact]
    public void IDto_EnablesTypeSafety_AtCompileTime()
    {
        // Arrange
        var dto = new DirectTestDto { Name = "Test" };

        // Act - This compiles successfully because of IDto constraint
        var result = GenericProcessor(dto);

        // Assert
        Assert.NotNull(result);
    }

    private string GenericProcessor<T>(T dto) where T : IDto
    {
        return dto.GetType().Name;
    }

    #endregion

    #region Hierarchy Validation Tests

    [Fact]
    public void AllDtoTypes_ImplementIDto()
    {
        // Arrange
        var types = new[]
        {
            typeof(DirectTestDto),
            typeof(CreateTestDto),
            typeof(EntityTestDto)
        };

        // Act & Assert
        foreach (var type in types)
        {
            Assert.True(typeof(IDto).IsAssignableFrom(type),
                $"{type.Name} should implement IDto");
        }
    }

    [Fact]
    public void IDto_IsAssignableFrom_AllDerivedInterfaces()
    {
        // Arrange
        var derivedInterfaces = new[]
        {
            typeof(ICreateDto)
        };

        // Act & Assert
        foreach (var interfaceType in derivedInterfaces)
        {
            Assert.True(typeof(IDto).IsAssignableFrom(interfaceType),
                $"{interfaceType.Name} should inherit from IDto");
        }
    }

    #endregion
}
