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
using System.Threading.Tasks;
using JumpStart.Data.Advanced;
using JumpStart.Data.Advanced.Auditing;
using JumpStart.Repositories.Advanced;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace JumpStart.Tests.Repositories.Advanced;

/// <summary>
/// Unit tests for the <see cref="Repository{TEntity, TKey}"/> class.
/// Tests abstract class structure, inheritance, and method signatures.
/// </summary>
public class RepositoryTests
{
    #region Test Classes

    /// <summary>
    /// Mock entity for testing.
    /// </summary>
    public class TestEntity : IEntity<int>
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    /// <summary>
    /// Mock auditable entity for testing.
    /// </summary>
    public class AuditableEntity : IEntity<int>, ICreatable<int>, IModifiable<int>, IDeletable<int>
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTimeOffset CreatedOn { get; set; }
        public int CreatedById { get; set; }
        public DateTimeOffset? ModifiedOn { get; set; }
        public int? ModifiedById { get; set; }
        public DateTimeOffset? DeletedOn { get; set; }
        public int? DeletedById { get; set; }
    }

    /// <summary>
    /// Mock DbContext for testing.
    /// </summary>
    public class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options)
            : base(options)
        {
        }

        public DbSet<TestEntity> TestEntities { get; set; } = null!;
        public DbSet<AuditableEntity> AuditableEntities { get; set; } = null!;
    }

    /// <summary>
    /// Concrete repository implementation for testing.
    /// </summary>
    public class TestRepository : Repository<TestEntity, int>
    {
        public TestRepository(DbContext context, IUserContext<int>? userContext = null)
            : base(context, userContext)
        {
        }

        // Expose protected members for testing
        public IQueryable<TestEntity> ApplySoftDeleteFilterPublic(IQueryable<TestEntity> query)
        {
            return ApplySoftDeleteFilter(query);
        }
    }

    /// <summary>
    /// Mock user context for testing.
    /// </summary>
    public class MockUserContext : IUserContext<int>
    {
        private readonly int? _userId;

        public MockUserContext(int? userId = 1)
        {
            _userId = userId;
        }

        public Task<int?> GetCurrentUserIdAsync()
        {
            return Task.FromResult(_userId);
        }
    }

    #endregion

    #region Class Characteristics Tests

    [Fact]
    public void Repository_IsAbstractClass()
    {
        // Arrange
        var repositoryType = typeof(Repository<,>);

        // Act & Assert
        Assert.True(repositoryType.IsAbstract);
        Assert.True(repositoryType.IsClass);
    }

    [Fact]
    public void Repository_IsPublic()
    {
        // Arrange
        var repositoryType = typeof(Repository<,>);

        // Act & Assert
        Assert.True(repositoryType.IsPublic);
    }

    [Fact]
    public void Repository_IsInCorrectNamespace()
    {
        // Arrange
        var repositoryType = typeof(Repository<,>);

        // Act & Assert
        Assert.Equal("JumpStart.Repositories.Advanced", repositoryType.Namespace);
    }

    [Fact]
    public void Repository_IsGeneric()
    {
        // Arrange
        var repositoryType = typeof(Repository<,>);

        // Act & Assert
        Assert.True(repositoryType.IsGenericType);
        Assert.Equal(2, repositoryType.GetGenericArguments().Length);
    }

    #endregion

    #region Generic Constraint Tests

    [Fact]
    public void Repository_TEntity_HasClassConstraint()
    {
        // Arrange
        var repositoryType = typeof(Repository<,>);
        var entityParameter = repositoryType.GetGenericArguments()[0];

        // Act
        var attributes = entityParameter.GenericParameterAttributes;

        // Assert
        Assert.True((attributes & GenericParameterAttributes.ReferenceTypeConstraint) != 0);
    }

    [Fact]
    public void Repository_TEntity_HasIEntityConstraint()
    {
        // Arrange
        var repositoryType = typeof(Repository<,>);
        var entityParameter = repositoryType.GetGenericArguments()[0];

        // Act
        var constraints = entityParameter.GetGenericParameterConstraints();

        // Assert
        Assert.Contains(constraints, c => c.IsGenericType && c.GetGenericTypeDefinition() == typeof(IEntity<>));
    }

    [Fact]
    public void Repository_TKey_HasStructConstraint()
    {
        // Arrange
        var repositoryType = typeof(Repository<,>);
        var keyParameter = repositoryType.GetGenericArguments()[1];

        // Act
        var attributes = keyParameter.GenericParameterAttributes;

        // Assert
        Assert.True((attributes & GenericParameterAttributes.NotNullableValueTypeConstraint) != 0);
    }

    #endregion

    #region Interface Implementation Tests

    [Fact]
    public void Repository_ImplementsIRepository()
    {
        // Arrange
        var repositoryType = typeof(Repository<TestEntity, int>);

        // Act
        var implementsInterface = typeof(IRepository<TestEntity, int>).IsAssignableFrom(repositoryType);

        // Assert
        Assert.True(implementsInterface);
    }

    #endregion

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullContext_ThrowsArgumentNullException()
    {
        // Arrange
        DbContext context = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new TestRepository(context));
    }

    #endregion

    #region Protected Field Tests

    [Fact]
    public void Repository_HasProtectedContextField()
    {
        // Arrange
        var repositoryType = typeof(Repository<,>);

        // Act
        var field = repositoryType.GetField("_context", BindingFlags.NonPublic | BindingFlags.Instance);

        // Assert
        Assert.NotNull(field);
        Assert.True(field!.IsFamily); // protected
    }

    [Fact]
    public void Repository_HasProtectedDbSetField()
    {
        // Arrange
        var repositoryType = typeof(Repository<,>);

        // Act
        var field = repositoryType.GetField("_dbSet", BindingFlags.NonPublic | BindingFlags.Instance);

        // Assert
        Assert.NotNull(field);
        Assert.True(field!.IsFamily); // protected
    }

    [Fact]
    public void Repository_HasProtectedUserContextField()
    {
        // Arrange
        var repositoryType = typeof(Repository<,>);

        // Act
        var field = repositoryType.GetField("_userContext", BindingFlags.NonPublic | BindingFlags.Instance);

        // Assert
        Assert.NotNull(field);
        Assert.True(field!.IsFamily); // protected
    }

    #endregion

    #region Method Signature Tests

    [Fact]
    public void Repository_HasGetByIdAsyncMethod()
    {
        // Arrange
        var repositoryType = typeof(Repository<TestEntity, int>);

        // Act
        var method = repositoryType.GetMethod(nameof(Repository<TestEntity, int>.GetByIdAsync));

        // Assert
        Assert.NotNull(method);
        Assert.True(method!.IsPublic);
        Assert.True(method.IsVirtual);
    }

    [Fact]
    public void Repository_HasGetAllAsyncMethod_NoParameters()
    {
        // Arrange
        var repositoryType = typeof(Repository<TestEntity, int>);

        // Act
        var method = repositoryType.GetMethod(nameof(Repository<TestEntity, int>.GetAllAsync), Type.EmptyTypes);

        // Assert
        Assert.NotNull(method);
        Assert.True(method!.IsPublic);
        Assert.True(method.IsVirtual);
    }

    [Fact]
    public void Repository_HasAddAsyncMethod()
    {
        // Arrange
        var repositoryType = typeof(Repository<TestEntity, int>);

        // Act
        var method = repositoryType.GetMethod(nameof(Repository<TestEntity, int>.AddAsync));

        // Assert
        Assert.NotNull(method);
        Assert.True(method!.IsPublic);
        Assert.True(method.IsVirtual);
    }

    [Fact]
    public void Repository_HasUpdateAsyncMethod()
    {
        // Arrange
        var repositoryType = typeof(Repository<TestEntity, int>);

        // Act
        var method = repositoryType.GetMethod(nameof(Repository<TestEntity, int>.UpdateAsync));

        // Assert
        Assert.NotNull(method);
        Assert.True(method!.IsPublic);
        Assert.True(method.IsVirtual);
    }

    [Fact]
    public void Repository_HasDeleteAsyncMethod()
    {
        // Arrange
        var repositoryType = typeof(Repository<TestEntity, int>);

        // Act
        var method = repositoryType.GetMethod(nameof(Repository<TestEntity, int>.DeleteAsync));

        // Assert
        Assert.NotNull(method);
        Assert.True(method!.IsPublic);
        Assert.True(method.IsVirtual);
    }

    [Fact]
    public void Repository_HasApplySoftDeleteFilterMethod()
    {
        // Arrange
        var repositoryType = typeof(Repository<,>);

        // Act
        var method = repositoryType.GetMethod("ApplySoftDeleteFilter", BindingFlags.NonPublic | BindingFlags.Instance);

        // Assert
        Assert.NotNull(method);
        Assert.True(method!.IsFamily); // protected
        Assert.True(method.IsVirtual);
    }

    #endregion

    #region Virtual Method Tests

    [Fact]
    public void AllPublicMethods_AreVirtual()
    {
        // Arrange
        var repositoryType = typeof(Repository<TestEntity, int>);
        var publicMethods = repositoryType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(m => !m.IsSpecialName); // Exclude property accessors

        // Act & Assert
        foreach (var method in publicMethods)
        {
            Assert.True(method.IsVirtual, $"Method {method.Name} should be virtual");
        }
    }

    #endregion

    #region Inheritance Tests

    [Fact]
    public void Repository_CanBeInherited()
    {
        // Arrange
        var repositoryType = typeof(TestRepository);
        var baseType = typeof(Repository<TestEntity, int>);

        // Act & Assert
        Assert.True(baseType.IsAssignableFrom(repositoryType));
    }

    #endregion

    #region Method Count Tests

    [Fact]
    public void Repository_HasSixPublicMethods()
    {
        // Arrange
        var repositoryType = typeof(Repository<TestEntity, int>);

        // Act
        var publicMethods = repositoryType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(m => !m.IsSpecialName) // Exclude property accessors
            .ToArray();

        // Assert - 6 methods from IRepository interface
        Assert.Equal(6, publicMethods.Length);
    }

    #endregion
}
