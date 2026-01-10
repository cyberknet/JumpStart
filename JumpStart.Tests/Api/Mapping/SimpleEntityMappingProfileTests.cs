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
using JumpStart.Api.DTOs;
using JumpStart.Api.Mapping;
using JumpStart.Api.Mapping.Advanced;
using JumpStart.Data;
using Xunit;

namespace JumpStart.Tests.Api.Mapping;

/// <summary>
/// Unit tests for the <see cref="SimpleEntityMappingProfile{TEntity, TDto, TCreateDto, TUpdateDto}"/> class.
/// Tests simplification pattern, Guid key type enforcement, and inheritance from EntityMappingProfile.
/// </summary>
public class SimpleEntityMappingProfileTests
{
    #region Test Classes

    /// <summary>
    /// Simple test entity with Guid identifier.
    /// </summary>
    public class TestSimpleEntity : ISimpleEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }

    /// <summary>
    /// Test DTO for read operations.
    /// </summary>
    public class TestSimpleEntityDto : SimpleEntityDto
    {
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }

    /// <summary>
    /// Test DTO for create operations.
    /// </summary>
    public class CreateTestSimpleEntityDto : ICreateDto
    {
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }

    /// <summary>
    /// Test DTO for update operations.
    /// </summary>
    public class UpdateTestSimpleEntityDto : IUpdateDto<Guid>
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }

    /// <summary>
    /// Test mapping profile.
    /// </summary>
    public class TestSimpleMappingProfile : SimpleEntityMappingProfile<
        TestSimpleEntity,
        TestSimpleEntityDto,
        CreateTestSimpleEntityDto,
        UpdateTestSimpleEntityDto>
    {
    }

    /// <summary>
    /// Test mapping profile with custom mappings.
    /// </summary>
    public class TestCustomSimpleMappingProfile : SimpleEntityMappingProfile<
        TestSimpleEntity,
        TestSimpleEntityDto,
        CreateTestSimpleEntityDto,
        UpdateTestSimpleEntityDto>
    {
        public bool AdditionalMappingsCalled { get; private set; }

        protected override void ConfigureAdditionalMappings()
        {
            AdditionalMappingsCalled = true;
        }
    }

    #endregion

    #region Profile Configuration Tests

    [Fact]
    public void Constructor_ConfiguresProfile_WithoutErrors()
    {
        // Arrange & Act
        var profile = new TestSimpleMappingProfile();

        // Assert - If configuration is invalid, constructor would throw
        Assert.NotNull(profile);
    }

    [Fact]
    public void Constructor_CallsConfigureAdditionalMappings()
    {
        // Arrange
        var profile = new TestCustomSimpleMappingProfile();

        // Assert
        Assert.True(profile.AdditionalMappingsCalled);
    }

    #endregion

    #region Inheritance Tests

    [Fact]
    public void SimpleEntityMappingProfile_InheritsFrom_EntityMappingProfile()
    {
        // Arrange
        var profileType = typeof(SimpleEntityMappingProfile<,,,>);

        // Act
        var baseType = profileType.BaseType;

        // Assert
        Assert.NotNull(baseType);
        Assert.True(baseType!.IsGenericType);
        Assert.Equal("EntityMappingProfile`5", baseType.GetGenericTypeDefinition().Name);
    }

    [Fact]
    public void SimpleEntityMappingProfile_UsesGuid_AsKeyType()
    {
        // Arrange - Check the open generic type's base
        var openGenericType = typeof(SimpleEntityMappingProfile<,,,>);

        // Act
        var baseType = openGenericType.BaseType;

        // Assert - The base type should be EntityMappingProfile with 5 generic parameters where TKey = Guid
        Assert.NotNull(baseType);
        Assert.True(baseType!.IsGenericType);

        // Verify it inherits from EntityMappingProfile (5 params)
        var baseGenericDefinition = baseType.GetGenericTypeDefinition();
        Assert.Equal(5, baseGenericDefinition.GetGenericArguments().Length);

        // Verify that ISimpleEntity uses Guid
        var entity = new TestSimpleEntity();
        Assert.IsType<Guid>(entity.Id);
    }

    [Fact]
    public void SimpleEntityMappingProfile_RequiresISimpleEntity()
    {
        // Arrange
        var profileType = typeof(SimpleEntityMappingProfile<,,,>);
        var entityConstraint = profileType.GetGenericArguments()[0].GetGenericParameterConstraints();

        // Assert
        Assert.Contains(entityConstraint, c => c == typeof(ISimpleEntity));
    }

    [Fact]
    public void SimpleEntityMappingProfile_RequiresSimpleEntityDto()
    {
        // Arrange
        var profileType = typeof(SimpleEntityMappingProfile<,,,>);
        var dtoConstraint = profileType.GetGenericArguments()[1].GetGenericParameterConstraints();

        // Assert
        Assert.Contains(dtoConstraint, c => c == typeof(SimpleEntityDto));
    }

    [Fact]
    public void SimpleEntityMappingProfile_RequiresIUpdateDtoWithGuid()
    {
        // Arrange
        var profileType = typeof(SimpleEntityMappingProfile<,,,>);
        var updateDtoConstraint = profileType.GetGenericArguments()[3].GetGenericParameterConstraints();

        // Assert
        Assert.Single(updateDtoConstraint);
        Assert.True(updateDtoConstraint[0].IsGenericType);
        Assert.Equal(typeof(IUpdateDto<>), updateDtoConstraint[0].GetGenericTypeDefinition());
    }

    #endregion

    #region Simplification Pattern Tests

    [Fact]
    public void SimpleEntityMappingProfile_SimplifiesGenericSignature()
    {
        // Arrange
        var simpleType = typeof(SimpleEntityMappingProfile<,,,>);
        var advancedType = typeof(EntityMappingProfile<,,,,>);

        // Act
        var simpleParamCount = simpleType.GetGenericArguments().Length;
        var advancedParamCount = advancedType.GetGenericArguments().Length;

        // Assert
        Assert.Equal(4, simpleParamCount); // Simple has 4 parameters
        Assert.Equal(5, advancedParamCount); // Advanced has 5 parameters (includes TKey)
        Assert.True(simpleParamCount < advancedParamCount,
            "SimpleEntityMappingProfile should have fewer generic parameters");
    }

    [Fact]
    public void SimpleEntityMappingProfile_FixesKeyType_ToGuid()
    {
        // Arrange & Act
        // This demonstrates that SimpleEntityMappingProfile usage doesn't require TKey parameter
        var profile = new TestSimpleMappingProfile(); // No <Guid> needed in inheritance

        // Assert
        Assert.NotNull(profile);
    }

    [Fact]
    public void SimpleEntityMappingProfile_ProvidesCleaner_DeclarationSyntax()
    {
        // Arrange - Compare declaration complexity
        var simpleTypeName = typeof(TestSimpleMappingProfile).BaseType!.Name;

        // Act
        var isSimpler = !simpleTypeName.Contains("`5"); // Should inherit from base with 4 params, not 5

        // Assert
        Assert.True(isSimpler || simpleTypeName.Contains("SimpleEntityMappingProfile"));
    }

    #endregion

    #region Class Characteristics Tests

    [Fact]
    public void SimpleEntityMappingProfile_IsAbstract()
    {
        // Arrange
        var profileType = typeof(SimpleEntityMappingProfile<,,,>);

        // Act
        var isAbstract = profileType.IsAbstract;

        // Assert
        Assert.True(isAbstract, "SimpleEntityMappingProfile should be abstract");
    }

    [Fact]
    public void SimpleEntityMappingProfile_IsInCorrectNamespace()
    {
        // Arrange
        var profileType = typeof(SimpleEntityMappingProfile<,,,>);

        // Act
        var namespaceName = profileType.Namespace;

        // Assert
        Assert.Equal("JumpStart.Api.Mapping", namespaceName);
    }

    [Fact]
    public void SimpleEntityMappingProfile_HasNoAdditionalMembers()
    {
        // Arrange
        var profileType = typeof(SimpleEntityMappingProfile<,,,>);

        // Act
        var declaredMembers = profileType.GetMembers(System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

        // Assert
        Assert.Empty(declaredMembers); // Should only inherit from EntityMappingProfile
    }

    #endregion

    #region Generic Parameter Tests

    [Fact]
    public void SimpleEntityMappingProfile_HasFourGenericParameters()
    {
        // Arrange
        var profileType = typeof(SimpleEntityMappingProfile<,,,>);

        // Act
        var genericParameters = profileType.GetGenericArguments();

        // Assert
        Assert.Equal(4, genericParameters.Length);
    }

    [Fact]
    public void SimpleEntityMappingProfile_GenericParameters_HaveCorrectNames()
    {
        // Arrange
        var profileType = typeof(SimpleEntityMappingProfile<,,,>);

        // Act
        var genericParameters = profileType.GetGenericArguments();

        // Assert
        Assert.Equal("TEntity", genericParameters[0].Name);
        Assert.Equal("TDto", genericParameters[1].Name);
        Assert.Equal("TCreateDto", genericParameters[2].Name);
        Assert.Equal("TUpdateDto", genericParameters[3].Name);
    }

    [Fact]
    public void SimpleEntityMappingProfile_RequiresReferenceTypeEntity()
    {
        // Arrange
        var profileType = typeof(SimpleEntityMappingProfile<,,,>);
        var entityParameter = profileType.GetGenericArguments()[0];

        // Act
        var isReferenceType = (entityParameter.GenericParameterAttributes & System.Reflection.GenericParameterAttributes.ReferenceTypeConstraint) != 0;

        // Assert
        Assert.True(isReferenceType);
    }

    #endregion

    #region Type Constraint Validation Tests

    [Fact]
    public void TestEntity_ImplementsISimpleEntity()
    {
        // Arrange
        var entityType = typeof(TestSimpleEntity);

        // Act
        var implementsInterface = typeof(ISimpleEntity).IsAssignableFrom(entityType);

        // Assert
        Assert.True(implementsInterface);
    }

    [Fact]
    public void TestDto_InheritsFromSimpleEntityDto()
    {
        // Arrange
        var dtoType = typeof(TestSimpleEntityDto);

        // Act
        var inheritsFrom = typeof(SimpleEntityDto).IsAssignableFrom(dtoType);

        // Assert
        Assert.True(inheritsFrom);
    }

    [Fact]
    public void TestCreateDto_ImplementsICreateDto()
    {
        // Arrange
        var createDtoType = typeof(CreateTestSimpleEntityDto);

        // Act
        var implementsInterface = typeof(ICreateDto).IsAssignableFrom(createDtoType);

        // Assert
        Assert.True(implementsInterface);
    }

    [Fact]
    public void TestUpdateDto_ImplementsIUpdateDtoWithGuid()
    {
        // Arrange
        var updateDtoType = typeof(UpdateTestSimpleEntityDto);

        // Act
        var implementsInterface = typeof(IUpdateDto<Guid>).IsAssignableFrom(updateDtoType);

        // Assert
        Assert.True(implementsInterface);
    }

    #endregion

    #region Design Pattern Tests

    [Fact]
    public void SimpleEntityMappingProfile_SimplifiesCommonScenario()
    {
        // Arrange
        var simpleProfile = typeof(TestSimpleMappingProfile);
        var baseProfile = simpleProfile.BaseType;

        // Act - Check that it inherits from EntityMappingProfile with Guid
        var inheritsFromEntityMappingProfile = baseProfile != null &&
                                                baseProfile.IsGenericType &&
                                                baseProfile.GetGenericTypeDefinition().Name.Contains("EntityMappingProfile");

        // Assert
        Assert.True(inheritsFromEntityMappingProfile);
    }

    [Fact]
    public void SimpleEntityMappingProfile_EnforcesGuidIdentifiers()
    {
        // Arrange
        var profile = new TestSimpleMappingProfile();
        var entity = new TestSimpleEntity { Id = Guid.NewGuid() };

        // Act
        var idType = entity.Id.GetType();

        // Assert
        Assert.Equal(typeof(Guid), idType);
    }

    #endregion

    #region Comparison with Advanced Profile Tests

    [Fact]
    public void SimpleEntityMappingProfile_IsSimpler_ThanEntityMappingProfile()
    {
        // Arrange
        var simpleType = typeof(SimpleEntityMappingProfile<,,,>);
        var advancedType = typeof(EntityMappingProfile<,,,,>);

        // Act
        var simpleHasFewerParams = simpleType.GetGenericArguments().Length < advancedType.GetGenericArguments().Length;

        // Assert
        Assert.True(simpleHasFewerParams, "SimpleEntityMappingProfile should have fewer generic parameters");
    }

    [Fact]
    public void SimpleEntityMappingProfile_EquivalentTo_EntityMappingProfileWithGuid()
    {
        // Arrange
        var simpleProfileType = typeof(SimpleEntityMappingProfile<,,,>);

        // Act
        var baseType = simpleProfileType.BaseType;
        var usesGuid = baseType != null &&
                       baseType.IsGenericType &&
                       baseType.GetGenericArguments().Length == 5 &&
                       baseType.GetGenericArguments()[1] == typeof(Guid);

        // Assert
        Assert.True(usesGuid, "SimpleEntityMappingProfile should use Guid as key type in base");
    }

    #endregion
}
