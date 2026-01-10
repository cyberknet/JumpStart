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
using JumpStart.Data.Advanced;
using Xunit;

namespace JumpStart.Tests.Data.Advanced;

/// <summary>
/// Unit tests for the <see cref="IUser{TKey}"/> interface.
/// Tests user entity identification, marker interface pattern, and audit tracking integration.
/// </summary>
public class IUserTests
{
    #region Test Classes

    /// <summary>
    /// Test user entity with int identifier.
    /// </summary>
    public class TestIntUser : IUser<int>
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    /// <summary>
    /// Test user entity with long identifier.
    /// </summary>
    public class TestLongUser : IUser<long>
    {
        public long Id { get; set; }
        public string Username { get; set; } = string.Empty;
    }

    /// <summary>
    /// Test user entity with Guid identifier.
    /// </summary>
    public class TestGuidUser : IUser<Guid>
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// Test non-user entity for comparison.
    /// </summary>
    public class TestDocument : IEntity<int>
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
    }

    #endregion

    #region Interface Characteristics Tests

    [Fact]
    public void IUser_IsInterface()
    {
        // Arrange
        var interfaceType = typeof(IUser<int>);

        // Act
        var isInterface = interfaceType.IsInterface;

        // Assert
        Assert.True(isInterface);
    }

    [Fact]
    public void IUser_IsPublic()
    {
        // Arrange
        var interfaceType = typeof(IUser<int>);

        // Act
        var isPublic = interfaceType.IsPublic;

        // Assert
        Assert.True(isPublic);
    }

    [Fact]
    public void IUser_IsInCorrectNamespace()
    {
        // Arrange
        var interfaceType = typeof(IUser<>);

        // Act
        var namespaceName = interfaceType.Namespace;

        // Assert
        Assert.Equal("JumpStart.Data.Advanced", namespaceName);
    }

    [Fact]
    public void IUser_InheritsFromIEntity()
    {
        // Arrange
        var interfaceType = typeof(IUser<int>);

        // Act
        var inheritsFromIEntity = typeof(IEntity<int>).IsAssignableFrom(interfaceType);

        // Assert
        Assert.True(inheritsFromIEntity);
    }

    [Fact]
    public void IUser_HasNoAdditionalMembers()
    {
        // Arrange
        var interfaceType = typeof(IUser<int>);

        // Act - Get members declared only in IUser (not inherited)
        var declaredMembers = interfaceType.GetMembers(System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

        // Assert - Should be empty (marker interface)
        Assert.Empty(declaredMembers);
    }

    #endregion

    #region Inheritance Tests

    [Fact]
    public void IUser_InheritsIdProperty_FromIEntity()
    {
        // Arrange
        var user = new TestIntUser();

        // Act
        user.Id = 123;

        // Assert
        Assert.Equal(123, user.Id);
    }

    [Fact]
    public void IUser_CanBeAssignedToIEntity()
    {
        // Arrange
        var user = new TestIntUser { Id = 1, Username = "testuser" };

        // Act
        IEntity<int> entity = user;

        // Assert
        Assert.NotNull(entity);
        Assert.Equal(1, entity.Id);
    }

    #endregion

    #region Generic Type Parameter Tests

    [Fact]
    public void IUser_WorksWithIntKey()
    {
        // Arrange & Act
        IUser<int> user = new TestIntUser
        {
            Id = 1,
            Username = "john"
        };

        // Assert
        Assert.Equal(1, user.Id);
        Assert.IsType<int>(user.Id);
    }

    [Fact]
    public void IUser_WorksWithLongKey()
    {
        // Arrange & Act
        IUser<long> user = new TestLongUser
        {
            Id = 1000000000L,
            Username = "jane"
        };

        // Assert
        Assert.Equal(1000000000L, user.Id);
        Assert.IsType<long>(user.Id);
    }

    [Fact]
    public void IUser_WorksWithGuidKey()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        IUser<Guid> user = new TestGuidUser
        {
            Id = id,
            Username = "admin"
        };

        // Assert
        Assert.Equal(id, user.Id);
        Assert.IsType<Guid>(user.Id);
    }

    [Fact]
    public void IUser_EnforcesStructConstraint()
    {
        // Arrange
        var interfaceType = typeof(IUser<>);
        var genericParameter = interfaceType.GetGenericArguments()[0];

        // Act
        var hasValueTypeConstraint = (genericParameter.GenericParameterAttributes & System.Reflection.GenericParameterAttributes.NotNullableValueTypeConstraint) != 0;

        // Assert
        Assert.True(hasValueTypeConstraint);
    }

    #endregion

    #region Implementation Tests

    [Fact]
    public void TestUser_ImplementsIUser()
    {
        // Arrange
        var user = new TestIntUser();

        // Act
        var implementsInterface = user is IUser<int>;

        // Assert
        Assert.True(implementsInterface);
    }

    [Fact]
    public void TestUser_CanBeAssignedToIUser()
    {
        // Arrange
        var user = new TestIntUser
        {
            Id = 1,
            Username = "testuser",
            Email = "test@example.com"
        };

        // Act
        IUser<int> iUser = user;

        // Assert
        Assert.NotNull(iUser);
        Assert.Equal(1, iUser.Id);
    }

    #endregion

    #region Marker Interface Pattern Tests

    [Fact]
    public void IUser_DistinguishesUserEntities_FromOtherEntities()
    {
        // Arrange
        var user = new TestIntUser { Id = 1 };
        var document = new TestDocument { Id = 1 };

        // Act
        var isUserEntity = user is IUser<int>;
        var isDocumentEntity = document is IEntity<int>;
        var isDocumentUser = document is IUser<int>;

        // Assert
        Assert.True(isUserEntity, "User should implement IUser<int>");
        Assert.True(isDocumentEntity, "Document should implement IEntity<int>");
        Assert.False(isDocumentUser, "Document should NOT implement IUser<int>");
    }

    [Fact]
    public void IUser_CanBeIdentified_InPolymorphicCollection()
    {
        // Arrange
        var entities = new List<IEntity<int>>
        {
            new TestIntUser { Id = 1, Username = "user1" },
            new TestDocument { Id = 2, Title = "doc1" },
            new TestIntUser { Id = 3, Username = "user2" }
        };

        // Act
        var users = entities.OfType<IUser<int>>().ToList();

        // Assert
        Assert.Equal(2, users.Count);
        Assert.All(users, u => Assert.True(u is IUser<int>));
    }

    #endregion

    #region Polymorphic Usage Tests

    [Fact]
    public void IUser_CanBeUsed_InCollections()
    {
        // Arrange
        var users = new List<IUser<int>>
        {
            new TestIntUser { Id = 1, Username = "alice" },
            new TestIntUser { Id = 2, Username = "bob" },
            new TestIntUser { Id = 3, Username = "charlie" }
        };

        // Act
        var userIds = users.Select(u => u.Id).ToList();

        // Assert
        Assert.Equal(3, userIds.Count);
        Assert.Contains(1, userIds);
        Assert.Contains(2, userIds);
        Assert.Contains(3, userIds);
    }

    [Fact]
    public void IUser_CanBeUsed_AsMethodParameter()
    {
        // Arrange
        var user = new TestIntUser { Id = 123, Username = "testuser" };

        // Act
        var userId = GetUserId(user);

        // Assert
        Assert.Equal(123, userId);
    }

    [Fact]
    public void IUser_CanBeUsed_AsGenericConstraint()
    {
        // Arrange
        var user = new TestIntUser { Id = 42, Username = "user42" };

        // Act
        var isValid = ValidateUser(user);

        // Assert
        Assert.True(isValid);
    }

    // Helper methods
    private int GetUserId<TKey>(IUser<TKey> user) where TKey : struct
    {
        return user.Id is int id ? id : 0;
    }

    private bool ValidateUser<TKey>(IUser<TKey> user) where TKey : struct
    {
        return !EqualityComparer<TKey>.Default.Equals(user.Id, default);
    }

    #endregion

    #region Audit Integration Pattern Tests

    [Fact]
    public void IUser_SupportsAuditTrackingPattern()
    {
        // Arrange - Simulate audit fields using same key type
        var user = new TestIntUser { Id = 5, Username = "auditor" };
        var createdById = user.Id; // Would be used in CreatedById field
        var modifiedById = user.Id; // Would be used in ModifiedById field

        // Act & Assert
        Assert.Equal(5, createdById);
        Assert.Equal(5, modifiedById);
        Assert.IsType<int>(createdById);
    }

    [Fact]
    public void IUser_TypeMatchesAuditFieldType()
    {
        // Arrange
        var user = new TestIntUser { Id = 10 };

        // Act - The user Id type should match audit field types
        var userId = user.Id;

        // Assert - Same type used in audit interfaces
        Assert.IsType<int>(userId);
    }

    #endregion

    #region LINQ Query Tests

    [Fact]
    public void IUser_SupportsLinqQueries_FilterById()
    {
        // Arrange
        var users = new List<IUser<int>>
        {
            new TestIntUser { Id = 1, Username = "user1" },
            new TestIntUser { Id = 2, Username = "user2" },
            new TestIntUser { Id = 3, Username = "user3" }
        };

        // Act
        var filteredUsers = users.Where(u => u.Id > 1).ToList();

        // Assert
        Assert.Equal(2, filteredUsers.Count);
        Assert.Contains(filteredUsers, u => u.Id == 2);
        Assert.Contains(filteredUsers, u => u.Id == 3);
    }

    [Fact]
    public void IUser_SupportsLinqQueries_OrderById()
    {
        // Arrange
        var users = new List<IUser<int>>
        {
            new TestIntUser { Id = 3 },
            new TestIntUser { Id = 1 },
            new TestIntUser { Id = 2 }
        };

        // Act
        var orderedUsers = users.OrderBy(u => u.Id).ToList();

        // Assert
        Assert.Equal(1, orderedUsers[0].Id);
        Assert.Equal(2, orderedUsers[1].Id);
        Assert.Equal(3, orderedUsers[2].Id);
    }

    #endregion

    #region Generic Service Pattern Tests

    [Fact]
    public void IUser_SupportsGenericServicePattern()
    {
        // Arrange
        var users = new List<TestIntUser>
        {
            new TestIntUser { Id = 1, Username = "alice" },
            new TestIntUser { Id = 2, Username = "bob" }
        };

        // Act
        var user = GetUserById(users, 1);

        // Assert
        Assert.NotNull(user);
        Assert.Equal(1, user!.Id);
        Assert.Equal("alice", user.Username);
    }

    // Helper method simulating generic service
    private TUser? GetUserById<TUser, TKey>(IEnumerable<TUser> users, TKey id)
        where TUser : IUser<TKey>
        where TKey : struct
    {
        return users.FirstOrDefault(u => EqualityComparer<TKey>.Default.Equals(u.Id, id));
    }

    #endregion

    #region Default Value Tests

    [Fact]
    public void NewUser_HasDefaultId()
    {
        // Arrange & Act
        var user = new TestIntUser();

        // Assert
        Assert.Equal(0, user.Id);
        Assert.Equal(default, user.Id);
    }

    [Fact]
    public void GuidUser_HasEmptyGuidByDefault()
    {
        // Arrange & Act
        var user = new TestGuidUser();

        // Assert
        Assert.Equal(Guid.Empty, user.Id);
        Assert.Equal(default, user.Id);
    }

    #endregion

    #region Mixed Type Tests

    [Fact]
    public void DifferentUserTypes_CanCoexistWithDifferentKeyTypes()
    {
        // Arrange & Act
        IUser<int> intUser = new TestIntUser { Id = 1 };
        IUser<long> longUser = new TestLongUser { Id = 1000L };
        IUser<Guid> guidUser = new TestGuidUser { Id = Guid.NewGuid() };

        // Assert
        Assert.IsType<int>(intUser.Id);
        Assert.IsType<long>(longUser.Id);
        Assert.IsType<Guid>(guidUser.Id);
    }

    #endregion
}
