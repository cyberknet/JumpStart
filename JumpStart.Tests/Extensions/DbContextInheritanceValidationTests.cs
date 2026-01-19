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
using JumpStart;
using JumpStart.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace JumpStart.Tests.Extensions;

/// <summary>
/// Unit tests for DbContext inheritance validation in ServiceCollectionExtensions.
/// </summary>
public class DbContextInheritanceValidationTests
{
    [Fact]
    public void AddJumpStart_WithValidDbContext_DoesNotThrow()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddDbContext<ValidDbContext>(options =>
            options.UseInMemoryDatabase("TestDb"));
        
        // Act & Assert
        var exception = Record.Exception(() => services.AddJumpStart());
        Assert.Null(exception);
    }
    
    [Fact]
    public void AddJumpStart_WithInvalidDbContext_ThrowsInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddDbContext<InvalidDbContext>(options =>
            options.UseInMemoryDatabase("TestDb"));

        // Act & Assert - Must enable AutoDiscoverRepositories for validation to occur
        var exception = Assert.Throws<InvalidOperationException>(() => 
            services.AddJumpStart(options => options.AutoDiscoverRepositories = true));
        Assert.Contains("must inherit from 'JumpStartDbContext'", exception.Message);
        Assert.Contains("InvalidDbContext", exception.Message);
    }
    
    [Fact]
    public void AddJumpStart_WithoutDbContext_DoesNotThrow()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert - No DbContext registered, validation should pass
        var exception = Record.Exception(() => services.AddJumpStart());
        Assert.Null(exception);
    }

    [Fact]
    public void AddJumpStart_WithInvalidDbContext_WithoutRepositories_DoesNotThrow()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddDbContext<InvalidDbContext>(options =>
            options.UseInMemoryDatabase("TestDb"));

        // Act & Assert - Repositories disabled, validation should be skipped
        var exception = Record.Exception(() => 
            services.AddJumpStart(options => options.AutoDiscoverRepositories = false));
        Assert.Null(exception);
    }
    
    [Fact]
    public void AddJumpStart_WithJumpStartDbContextDirectly_DoesNotThrow()
    {
        // Arrange - This shouldn't happen in practice, but test it anyway
        var services = new ServiceCollection();
        services.AddDbContext<JumpStartDbContext>(options =>
            options.UseInMemoryDatabase("TestDb"));
        
        // Act & Assert
        var exception = Record.Exception(() => services.AddJumpStart());
        Assert.Null(exception);
    }
    
    [Fact]
    public void AddJumpStart_WithMultipleValidDbContexts_DoesNotThrow()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddDbContext<ValidDbContext>(options =>
            options.UseInMemoryDatabase("TestDb1"));
        services.AddDbContext<AnotherValidDbContext>(options =>
            options.UseInMemoryDatabase("TestDb2"));
        
        // Act & Assert
        var exception = Record.Exception(() => services.AddJumpStart());
        Assert.Null(exception);
    }
    
    [Fact]
    public void AddJumpStart_WithMixedDbContexts_ThrowsForInvalidOne()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddDbContext<ValidDbContext>(options =>
            options.UseInMemoryDatabase("TestDb1"));
        services.AddDbContext<InvalidDbContext>(options =>
            options.UseInMemoryDatabase("TestDb2"));

        // Act & Assert - Must enable AutoDiscoverRepositories for validation to occur
        var exception = Assert.Throws<InvalidOperationException>(() => 
            services.AddJumpStart(options => options.AutoDiscoverRepositories = true));
        Assert.Contains("InvalidDbContext", exception.Message);
    }
    
    [Fact]
    public void AddJumpStart_ExceptionMessageIncludesDocumentationLink()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddDbContext<InvalidDbContext>(options =>
            options.UseInMemoryDatabase("TestDb"));

        // Act & Assert - Must enable AutoDiscoverRepositories for validation to occur
        var exception = Assert.Throws<InvalidOperationException>(() => 
            services.AddJumpStart(options => options.AutoDiscoverRepositories = true));
        Assert.Contains("https://github.com/cyberknet/JumpStart", exception.Message);
    }
    
    [Fact]
    public void AddJumpStart_ExceptionMessageIncludesFixInstructions()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddDbContext<InvalidDbContext>(options =>
            options.UseInMemoryDatabase("TestDb"));

        // Act & Assert - Must enable AutoDiscoverRepositories for validation to occur
        var exception = Assert.Throws<InvalidOperationException>(() => 
            services.AddJumpStart(options => options.AutoDiscoverRepositories = true));
        Assert.Contains("Change your DbContext declaration", exception.Message);
        Assert.Contains("public class InvalidDbContext : JumpStartDbContext", exception.Message);
    }
    
    #region Helper Classes
    
    /// <summary>
    /// Valid DbContext that inherits from JumpStartDbContext.
    /// </summary>
    private class ValidDbContext : JumpStartDbContext
    {
        public ValidDbContext(DbContextOptions<ValidDbContext> options)
            : base(options)
        {
        }
    }
    
    /// <summary>
    /// Another valid DbContext that inherits from JumpStartDbContext.
    /// </summary>
    private class AnotherValidDbContext : JumpStartDbContext
    {
        public AnotherValidDbContext(DbContextOptions<AnotherValidDbContext> options)
            : base(options)
        {
        }
    }
    
    /// <summary>
    /// Invalid DbContext that does NOT inherit from JumpStartDbContext.
    /// </summary>
    private class InvalidDbContext : DbContext
    {
        public InvalidDbContext(DbContextOptions<InvalidDbContext> options)
            : base(options)
        {
        }
    }
    
    #endregion
}
