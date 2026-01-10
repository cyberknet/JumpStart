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
using System.Reflection;
using JumpStart.Api.Clients;
using JumpStart.Api.Clients.Advanced;
using JumpStart.Api.DTOs;
using Xunit;

namespace JumpStart.Tests.Api.Clients;

/// <summary>
/// Unit tests for the <see cref="ISimpleApiClient{TDto, TCreateDto, TUpdateDto}"/> interface.
/// Tests interface inheritance, type constraints, and marker interface pattern implementation.
/// </summary>
public class ISimpleApiClientTests
{
    #region Test DTOs

    /// <summary>
    /// Valid test DTO inheriting from SimpleEntityDto.
    /// </summary>
    public class ValidTestDto : SimpleEntityDto
    {
        public string Name { get; set; } = string.Empty;
    }

    /// <summary>
    /// Valid create DTO implementing ICreateDto.
    /// </summary>
    public class ValidCreateDto : ICreateDto
    {
        public string Name { get; set; } = string.Empty;
    }

    /// <summary>
    /// Valid update DTO implementing IUpdateDto with Guid.
    /// </summary>
    public class ValidUpdateDto : IUpdateDto<Guid>
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    /// <summary>
    /// Test interface properly implementing ISimpleApiClient.
    /// </summary>
    public interface IValidTestClient : ISimpleApiClient<ValidTestDto, ValidCreateDto, ValidUpdateDto>
    {
    }

    #endregion

    #region Interface Inheritance Tests

    [Fact]
    public void ISimpleApiClient_InheritsFrom_IAdvancedApiClient()
    {
        // Arrange
        var simpleClientType = typeof(ISimpleApiClient<ValidTestDto, ValidCreateDto, ValidUpdateDto>);
        var advancedClientType = typeof(IAdvancedApiClient<ValidTestDto, ValidCreateDto, ValidUpdateDto, Guid>);

        // Act
        var inheritsFromAdvanced = advancedClientType.IsAssignableFrom(simpleClientType);

        // Assert
        Assert.True(inheritsFromAdvanced, "ISimpleApiClient should inherit from IAdvancedApiClient with Guid key type");
    }

    [Fact]
    public void ISimpleApiClient_HasCorrectGenericParameters()
    {
        // Arrange
        var interfaceType = typeof(ISimpleApiClient<,,>);

        // Act
        var genericArguments = interfaceType.GetGenericArguments();

        // Assert
        Assert.Equal(3, genericArguments.Length);
        Assert.Equal("TDto", genericArguments[0].Name);
        Assert.Equal("TCreateDto", genericArguments[1].Name);
        Assert.Equal("TUpdateDto", genericArguments[2].Name);
    }

    [Fact]
    public void ISimpleApiClient_HasCorrectTypeConstraints()
    {
        // Arrange
        var interfaceType = typeof(ISimpleApiClient<,,>);
        var genericArguments = interfaceType.GetGenericArguments();

        // Act
        var dtoConstraints = genericArguments[0].GetGenericParameterConstraints();
        var createDtoConstraints = genericArguments[1].GetGenericParameterConstraints();
        var updateDtoConstraints = genericArguments[2].GetGenericParameterConstraints();

        // Assert - TDto must inherit from SimpleEntityDto
        Assert.Single(dtoConstraints);
        Assert.Equal(typeof(SimpleEntityDto), dtoConstraints[0]);

        // Assert - TCreateDto must implement ICreateDto
        Assert.Single(createDtoConstraints);
        Assert.Equal(typeof(ICreateDto), createDtoConstraints[0]);

        // Assert - TUpdateDto must implement IUpdateDto<Guid>
        Assert.Single(updateDtoConstraints);
        Assert.True(updateDtoConstraints[0].IsGenericType);
        Assert.Equal(typeof(IUpdateDto<>), updateDtoConstraints[0].GetGenericTypeDefinition());
    }

    #endregion

    #region Marker Interface Pattern Tests

    [Fact]
    public void ISimpleApiClient_IsMarkerInterface_NoAdditionalMembers()
    {
        // Arrange
        var interfaceType = typeof(ISimpleApiClient<ValidTestDto, ValidCreateDto, ValidUpdateDto>);

        // Act
        var declaredMembers = interfaceType.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        // Assert
        Assert.Empty(declaredMembers); // Marker interface has no declared members
    }

    [Fact]
    public void ISimpleApiClient_InheritsAllMembers_FromIAdvancedApiClient()
    {
        // Arrange
        var simpleClientType = typeof(ISimpleApiClient<ValidTestDto, ValidCreateDto, ValidUpdateDto>);

        // Act
        var interfaces = simpleClientType.GetInterfaces();
        var baseInterface = interfaces.FirstOrDefault();

        // Assert
        Assert.Single(interfaces); // Should only inherit from IAdvancedApiClient
        Assert.NotNull(baseInterface);
        Assert.True(baseInterface!.IsGenericType);
        Assert.Equal(typeof(IAdvancedApiClient<,,,>), baseInterface.GetGenericTypeDefinition());
    }

    [Fact]
    public void ISimpleApiClient_ProvidesTypeAlias_ForGuidBasedClients()
    {
        // Arrange
        var simpleClientType = typeof(ISimpleApiClient<ValidTestDto, ValidCreateDto, ValidUpdateDto>);
        var advancedClientType = typeof(IAdvancedApiClient<ValidTestDto, ValidCreateDto, ValidUpdateDto, Guid>);

        // Act
        var baseInterface = simpleClientType.GetInterfaces().FirstOrDefault();

        // Assert
        Assert.NotNull(baseInterface);
        Assert.Equal(advancedClientType, baseInterface);
    }

    #endregion

    #region Type Safety Tests

    [Fact]
    public void ISimpleApiClient_EnforcesGuidKeyType()
    {
        // Arrange
        var interfaceType = typeof(ISimpleApiClient<ValidTestDto, ValidCreateDto, ValidUpdateDto>);
        var baseInterface = interfaceType.GetInterfaces()[0]; // IAdvancedApiClient

        // Act
        var keyTypeArgument = baseInterface.GetGenericArguments()[3]; // 4th generic parameter is TKey

        // Assert
        Assert.Equal(typeof(Guid), keyTypeArgument);
    }

    [Fact]
    public void ISimpleApiClient_CanBeImplemented_ByConcreteInterface()
    {
        // Arrange & Act
        var testClientType = typeof(IValidTestClient);
        var simpleClientType = typeof(ISimpleApiClient<ValidTestDto, ValidCreateDto, ValidUpdateDto>);

        // Assert
        Assert.True(simpleClientType.IsAssignableFrom(testClientType));
    }

    [Fact]
    public void ISimpleApiClient_InheritedInterface_IncludesAdvancedApiClientMethods()
    {
        // Arrange
        var interfaceType = typeof(ISimpleApiClient<ValidTestDto, ValidCreateDto, ValidUpdateDto>);
        var baseInterfaceType = typeof(IAdvancedApiClient<,,,>);

        // Act
        var baseInterface = interfaceType.GetInterfaces().FirstOrDefault();

        // Assert - Verify inheritance structure
        Assert.NotNull(baseInterface);
        Assert.True(baseInterface!.IsGenericType);
        Assert.Equal(baseInterfaceType, baseInterface.GetGenericTypeDefinition());

        // Verify the base interface has the expected methods defined
        var baseInterfaceMethods = baseInterfaceType.GetMethods();
        Assert.Contains(baseInterfaceMethods, m => m.Name == "GetByIdAsync");
        Assert.Contains(baseInterfaceMethods, m => m.Name == "GetAllAsync");
        Assert.Contains(baseInterfaceMethods, m => m.Name == "CreateAsync");
        Assert.Contains(baseInterfaceMethods, m => m.Name == "UpdateAsync");
        Assert.Contains(baseInterfaceMethods, m => m.Name == "DeleteAsync");
    }

    #endregion

    #region Interface Properties Tests

    [Fact]
    public void ISimpleApiClient_IsPublicInterface()
    {
        // Arrange
        var interfaceType = typeof(ISimpleApiClient<,,>);

        // Act
        var isPublic = interfaceType.IsPublic;
        var isInterface = interfaceType.IsInterface;

        // Assert
        Assert.True(isPublic);
        Assert.True(isInterface);
    }

    [Fact]
    public void ISimpleApiClient_IsInCorrectNamespace()
    {
        // Arrange
        var interfaceType = typeof(ISimpleApiClient<,,>);

        // Act
        var namespaceName = interfaceType.Namespace;

        // Assert
        Assert.Equal("JumpStart.Api.Clients", namespaceName);
    }

    [Fact]
    public void ISimpleApiClient_HasCorrectFullName()
    {
        // Arrange
        var interfaceType = typeof(ISimpleApiClient<,,>);

        // Act
        var fullName = interfaceType.FullName;

        // Assert
        Assert.Contains("JumpStart.Api.Clients.ISimpleApiClient", fullName!);
    }

    #endregion

    #region Documentation and Naming Tests

    [Fact]
    public void ISimpleApiClient_FollowsNamingConvention()
    {
        // Arrange
        var interfaceType = typeof(ISimpleApiClient<,,>);

        // Act
        var name = interfaceType.Name;

        // Assert
        Assert.StartsWith("I", name); // Interface naming convention
        Assert.Contains("Simple", name); // Indicates Guid-based simplification
        Assert.Contains("ApiClient", name); // Clear purpose
        Assert.DoesNotContain("Guid", name); // Abstracts away the key type
    }

    [Fact]
    public void ISimpleApiClient_UsesFullWords_NoAbbreviations()
    {
        // Arrange
        var interfaceType = typeof(ISimpleApiClient<,,>);
        var name = interfaceType.Name.Replace("`3", ""); // Remove generic arity marker

        // Assert
        Assert.Contains("Simple", name); // Full word (not Simp)
        Assert.Contains("ApiClient", name); // Full words (not API or Clt)
        // Note: "Simple" contains "Simp" but that's acceptable as it's the full word
    }

    #endregion

    #region Design Pattern Validation Tests

    [Fact]
    public void ISimpleApiClient_SimplifiesGenericSignature_ByFixingKeyType()
    {
        // Arrange
        var simpleClientType = typeof(ISimpleApiClient<ValidTestDto, ValidCreateDto, ValidUpdateDto>);
        var advancedClientType = typeof(IAdvancedApiClient<ValidTestDto, ValidCreateDto, ValidUpdateDto, Guid>);

        // Act
        var simpleGenericArgs = simpleClientType.GetGenericArguments().Length;
        var advancedGenericArgs = advancedClientType.GetGenericArguments().Length;

        // Assert
        Assert.Equal(3, simpleGenericArgs); // Simplified: only 3 type parameters
        Assert.Equal(4, advancedGenericArgs); // Advanced: 4 type parameters
        Assert.True(simpleGenericArgs < advancedGenericArgs, 
            "ISimpleApiClient should have fewer generic parameters than IAdvancedApiClient");
    }

    [Fact]
    public void ISimpleApiClient_ProvidesConvenience_ForCommonScenario()
    {
        // Arrange - Show that simple client is easier to use
        var simpleClientTypeDef = typeof(ISimpleApiClient<,,>);
        var advancedClientTypeDef = typeof(IAdvancedApiClient<,,,>);
        var simpleClientClosed = typeof(ISimpleApiClient<ValidTestDto, ValidCreateDto, ValidUpdateDto>);
        var advancedClientClosed = typeof(IAdvancedApiClient<ValidTestDto, ValidCreateDto, ValidUpdateDto, Guid>);

        // Act
        var simpleGenericParamCount = simpleClientTypeDef.GetGenericArguments().Length;
        var advancedGenericParamCount = advancedClientTypeDef.GetGenericArguments().Length;
        var areEquivalent = advancedClientClosed.IsAssignableFrom(simpleClientClosed);

        // Assert
        Assert.True(areEquivalent, "Both interfaces should be functionally equivalent");
        Assert.Equal(3, simpleGenericParamCount); // Simple has 3 parameters
        Assert.Equal(4, advancedGenericParamCount); // Advanced has 4 parameters
    }

    #endregion
}
