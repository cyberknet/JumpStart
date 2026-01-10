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
using JumpStart.Data;
using JumpStart.Data.Advanced;
using Xunit;

namespace JumpStart.Tests.Data;

/// <summary>
/// Unit tests for the <see cref="ISimpleUser"/> interface.
/// Tests Guid-based user identification, inheritance, and usage patterns.
/// </summary>
public class ISimpleUserTests
{
    #region Test Classes

    /// <summary>
    /// Test user entity implementing ISimpleUser.
    /// </summary>
    public class TestUser : ISimpleEntity, ISimpleUser
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// Another test user entity.
    /// </summary>
    public class TestAdministrator : ISimpleEntity, ISimpleUser
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string[] Roles { get; set; } = Array.Empty<string>();
    }

    #endregion

    #region Interface Characteristics Tests

    [Fact]
    public void ISimpleUser_IsInterface()
    {
        // Arrange
        var interfaceType = typeof(ISimpleUser);

        // Act
        var isInterface = interfaceType.IsInterface;

        // Assert
        Assert.True(isInterface);
    }

    [Fact]
    public void ISimpleUser_IsPublic()
    {
        // Arrange
        var interfaceType = typeof(ISimpleUser);

        // Act
        var isPublic = interfaceType.IsPublic;

        // Assert
        Assert.True(isPublic);
    }

    [Fact]
    public void ISimpleUser_IsInCorrectNamespace()
    {
        // Arrange
        var interfaceType = typeof(ISimpleUser);

        // Act
        var namespaceName = interfaceType.Namespace;

        // Assert
        Assert.Equal("JumpStart.Data", namespaceName);
    }

    [Fact]
    public void ISimpleUser_HasNoMembers()
    {
        // Arrange
        var interfaceType = typeof(ISimpleUser);

        // Act
        var declaredMembers = interfaceType.GetMembers(System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

        // Assert
        Assert.Empty(declaredMembers); // Marker interface
    }

    #endregion

    #region Inheritance Tests

    [Fact]
    public void ISimpleUser_InheritsFrom_IUser()
    {
        // Arrange
        var interfaceType = typeof(ISimpleUser);

        // Act
        var inheritsFromIUser = typeof(IUser<Guid>).IsAssignableFrom(interfaceType);

        // Assert
        Assert.True(inheritsFromIUser);
    }

    [Fact]
    public void ISimpleUser_UsesGuidAsTypeParameter()
    {
        // Arrange
        var interfaceType = typeof(ISimpleUser);
        var baseInterface = interfaceType.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IUser<>));

        // Act
        var typeArgument = baseInterface?.GetGenericArguments()[0];

        // Assert
        Assert.NotNull(typeArgument);
        Assert.Equal(typeof(Guid), typeArgument);
    }

    [Fact]
    public void ISimpleUser_InheritsFrom_IEntity()
    {
        // Arrange
        var interfaceType = typeof(ISimpleUser);

        // Act
        // IUser<Guid> inherits from IEntity<Guid>, so ISimpleUser should too
        var inheritsFromIEntity = typeof(IEntity<Guid>).IsAssignableFrom(interfaceType);

        // Assert
        Assert.True(inheritsFromIEntity);
    }

    #endregion

    #region Property Tests

    [Fact]
    public void Id_IsGuidType()
    {
        // Arrange
        var user = new TestUser
        {
            Id = Guid.NewGuid()
        };

        // Act & Assert
        Assert.IsType<Guid>(user.Id);
    }

    [Fact]
    public void Id_CanBeSet_AndRetrieved()
    {
        // Arrange
        var user = new TestUser();
        var id = Guid.NewGuid();

        // Act
        user.Id = id;

        // Assert
        Assert.Equal(id, user.Id);
    }

    #endregion

    #region Implementation Tests

    [Fact]
    public void TestUser_ImplementsISimpleUser()
    {
        // Arrange
        var user = new TestUser();

        // Act
        var implementsInterface = user is ISimpleUser;

        // Assert
        Assert.True(implementsInterface);
    }

    [Fact]
    public void TestUser_CanBeAssignedToISimpleUser()
    {
        // Arrange
        var user = new TestUser
        {
            Id = Guid.NewGuid(),
            Username = "testuser"
        };

        // Act
        ISimpleUser simpleUser = user;

        // Assert
        Assert.NotNull(simpleUser);
        Assert.NotEqual(Guid.Empty, simpleUser.Id);
    }

    [Fact]
    public void TestUser_CanBeAssignedToIUserGuid()
    {
        // Arrange
        var user = new TestUser
        {
            Id = Guid.NewGuid()
        };

        // Act
        IUser<Guid> iUser = user;

        // Assert
        Assert.NotNull(iUser);
        Assert.NotEqual(Guid.Empty, iUser.Id);
    }

    [Fact]
    public void TestUser_ImplementsISimpleEntity()
    {
        // Arrange
        var user = new TestUser();

        // Act
        var implementsISimpleEntity = user is ISimpleEntity;

        // Assert
        Assert.True(implementsISimpleEntity);
    }

    #endregion

    #region Polymorphic Usage Tests

    [Fact]
    public void ISimpleUser_CanBeUsed_InCollections()
    {
        // Arrange
        var users = new List<ISimpleUser>
        {
            new TestUser { Id = Guid.NewGuid(), Username = "user1" },
            new TestAdministrator { Id = Guid.NewGuid(), Username = "admin1" }
        };

        // Act
        var ids = users.Select(u => u.Id).ToList();

        // Assert
        Assert.Equal(2, ids.Count);
        Assert.All(ids, id => Assert.NotEqual(Guid.Empty, id));
    }

    [Fact]
    public void ISimpleUser_CanBeUsed_AsMethodParameter()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new TestUser
        {
            Id = userId,
            Username = "testuser"
        };

        // Act
        var retrievedId = GetUserId(user);

        // Assert
        Assert.Equal(userId, retrievedId);
    }

    [Fact]
    public void ISimpleUser_CanBeUsed_WithGenericConstraints()
    {
        // Arrange
        var users = new[]
        {
            new TestUser { Id = Guid.NewGuid(), Username = "user1" },
            new TestUser { Id = Guid.NewGuid(), Username = "user2" }
        };

        // Act
        var userIds = GetAllUserIds(users);

        // Assert
        Assert.Equal(2, userIds.Count);
    }

    // Helper methods
    private Guid GetUserId(ISimpleUser user)
    {
        return user.Id;
    }

    private List<Guid> GetAllUserIds<TUser>(IEnumerable<TUser> users)
        where TUser : ISimpleUser
    {
        return users.Select(u => u.Id).ToList();
    }

    #endregion

    #region User Filtering Tests

    [Fact]
    public void ISimpleUser_SupportsFiltering_ById()
    {
        // Arrange
        var targetId = Guid.NewGuid();
        var users = new List<ISimpleUser>
        {
            new TestUser { Id = Guid.NewGuid(), Username = "user1" },
            new TestUser { Id = targetId, Username = "target" },
            new TestUser { Id = Guid.NewGuid(), Username = "user2" }
        };

        // Act
        var found = users.FirstOrDefault(u => u.Id == targetId);

        // Assert
        Assert.NotNull(found);
        Assert.Equal(targetId, found!.Id);
    }

    [Fact]
    public void ISimpleUser_SupportsFiltering_ByMultipleIds()
    {
        // Arrange
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var targetIds = new[] { id1, id2 };
        
        var users = new List<ISimpleUser>
        {
            new TestUser { Id = id1 },
            new TestUser { Id = Guid.NewGuid() },
            new TestUser { Id = id2 }
        };

        // Act
        var filtered = users.Where(u => targetIds.Contains(u.Id)).ToList();

        // Assert
        Assert.Equal(2, filtered.Count);
    }

    #endregion

    #region Marker Interface Tests

    [Fact]
    public void ISimpleUser_DistinguishesUserEntities_FromOtherEntities()
    {
        // Arrange
        var user = new TestUser { Id = Guid.NewGuid() };
        ISimpleEntity entity = user;

        // Act
        var isUser = entity is ISimpleUser;

        // Assert
        Assert.True(isUser);
    }

    [Fact]
    public void ISimpleUser_ProvidesTypeSafety_ForUserReferences()
    {
        // Arrange
        var user = new TestUser { Id = Guid.NewGuid(), Username = "testuser" };

        // Act - Compile-time type safety
        ISimpleUser typedUser = user;
        
        // Assert
        Assert.NotNull(typedUser);
        Assert.IsAssignableFrom<ISimpleUser>(user);
    }

    #endregion

    #region Type Alias Pattern Tests

    [Fact]
    public void ISimpleUser_SimplifiesGenericConstraint()
    {
        // Arrange
        var user = new TestUser { Id = Guid.NewGuid() };

        // Act - Use simplified constraint
        var result = ProcessUser(user);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ISimpleUser_CanBeUsedInsteadOfIUserGuid()
    {
        // Arrange
        var user = new TestUser { Id = Guid.NewGuid() };

        // Act - Both work
        ISimpleUser simple = user;
        IUser<Guid> generic = user;

        // Assert
        Assert.Same(simple, generic);
    }

    // Helper method demonstrating simplified API
    private bool ProcessUser<TUser>(TUser user)
        where TUser : ISimpleUser
    {
        return user.Id != Guid.Empty;
    }

    #endregion

    #region Audit Reference Tests

    [Fact]
    public void ISimpleUser_CanBeReferenced_InAuditFields()
    {
        // Arrange
        var creatorId = Guid.NewGuid();
        var modifierId = Guid.NewGuid();
        
        var creator = new TestUser { Id = creatorId, Username = "creator" };
        var modifier = new TestUser { Id = modifierId, Username = "modifier" };

        // Act
        ISimpleUser creatorRef = creator;
        ISimpleUser modifierRef = modifier;

        // Assert
        Assert.Equal(creatorId, creatorRef.Id);
        Assert.Equal(modifierId, modifierRef.Id);
    }

    #endregion

    #region Guid Benefits Tests

    [Fact]
    public void ISimpleUser_Guid_ProvidesGlobalUniqueness()
    {
        // Arrange & Act
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();

        // Assert
        Assert.NotEqual(userId1, userId2);
    }

    [Fact]
    public void ISimpleUser_Guid_CanBeGeneratedClientSide()
    {
        // Arrange & Act
        var user = new TestUser
        {
            Id = Guid.NewGuid(),
            Username = "newuser"
        };

        // Assert
        Assert.NotEqual(Guid.Empty, user.Id);
    }

    #endregion
}
