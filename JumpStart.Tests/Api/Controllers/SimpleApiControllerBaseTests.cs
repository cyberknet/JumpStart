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
using System.Threading.Tasks;
using AutoMapper;
using JumpStart.Api.Controllers;
using JumpStart.Api.Controllers.Advanced;
using JumpStart.Api.DTOs;
using JumpStart.Data;
using JumpStart.Repositories;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace JumpStart.Tests.Api.Controllers;

/// <summary>
/// Unit tests for the <see cref="SimpleApiControllerBase{TEntity, TDto, TCreateDto, TUpdateDto, TRepository}"/> class.
/// Tests inheritance from AdvancedApiControllerBase, Guid key type enforcement, and basic CRUD operations.
/// </summary>
public class SimpleApiControllerBaseTests
{
    #region Test Classes

    /// <summary>
    /// Test entity implementing ISimpleEntity (Guid-based).
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
    /// Concrete test controller for testing base controller functionality.
    /// </summary>
    public class TestSimpleController : SimpleApiControllerBase<
        TestSimpleEntity,
        TestSimpleEntityDto,
        CreateTestSimpleEntityDto,
        UpdateTestSimpleEntityDto,
        ISimpleRepository<TestSimpleEntity>>
    {
        public TestSimpleController(ISimpleRepository<TestSimpleEntity> repository, IMapper mapper)
            : base(repository, mapper)
        {
        }
    }

    #endregion

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidArguments_CreatesInstance()
    {
        // Arrange
        var mockRepository = new Mock<ISimpleRepository<TestSimpleEntity>>();
        var mockMapper = new Mock<IMapper>();

        // Act
        var controller = new TestSimpleController(mockRepository.Object, mockMapper.Object);

        // Assert
        Assert.NotNull(controller);
    }

    [Fact]
    public void Constructor_WithNullRepository_ThrowsArgumentNullException()
    {
        // Arrange
        var mockMapper = new Mock<IMapper>();

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new TestSimpleController(null!, mockMapper.Object));
        Assert.Equal("repository", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullMapper_ThrowsArgumentNullException()
    {
        // Arrange
        var mockRepository = new Mock<ISimpleRepository<TestSimpleEntity>>();

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new TestSimpleController(mockRepository.Object, null!));
        Assert.Equal("mapper", exception.ParamName);
    }

    #endregion

    #region Inheritance Tests

    [Fact]
    public void SimpleApiControllerBase_InheritsFrom_AdvancedApiControllerBase()
    {
        // Arrange
        var simpleType = typeof(SimpleApiControllerBase<,,,,>);

        // Act
        var baseType = simpleType.BaseType;

        // Assert
        Assert.NotNull(baseType);
        Assert.True(baseType!.IsGenericType);
        Assert.Equal("AdvancedApiControllerBase`6", baseType.GetGenericTypeDefinition().Name);
    }

    [Fact]
    public void SimpleApiControllerBase_UsesGuidAsKeyType()
    {
        // Arrange - Use the closed generic type
        var closedType = typeof(TestSimpleController);

        // Act - Get the base type (SimpleApiControllerBase) and then its base type (AdvancedApiControllerBase)
        var simpleBase = closedType.BaseType;
        Assert.NotNull(simpleBase);

        var advancedBase = simpleBase!.BaseType;
        Assert.NotNull(advancedBase);
        Assert.True(advancedBase!.IsGenericType);

        var genericArguments = advancedBase.GetGenericArguments();
        var keyType = genericArguments[1]; // Second generic argument is TKey in AdvancedApiControllerBase

        // Assert
        Assert.Equal(typeof(Guid), keyType);
    }

    [Fact]
    public void SimpleApiControllerBase_RequiresISimpleEntity()
    {
        // Arrange
        var controllerBaseType = typeof(SimpleApiControllerBase<,,,,>);
        var entityConstraint = controllerBaseType.GetGenericArguments()[0].GetGenericParameterConstraints();

        // Act & Assert
        Assert.Contains(entityConstraint, c => c == typeof(ISimpleEntity));
    }

    [Fact]
    public void SimpleApiControllerBase_RequiresSimpleEntityDto()
    {
        // Arrange
        var controllerBaseType = typeof(SimpleApiControllerBase<,,,,>);
        var dtoConstraint = controllerBaseType.GetGenericArguments()[1].GetGenericParameterConstraints();

        // Act & Assert
        Assert.Contains(dtoConstraint, c => c == typeof(SimpleEntityDto));
    }

    [Fact]
    public void SimpleApiControllerBase_RequiresISimpleRepository()
    {
        // Arrange
        var controllerBaseType = typeof(SimpleApiControllerBase<,,,,>);
        var repoConstraint = controllerBaseType.GetGenericArguments()[4].GetGenericParameterConstraints();

        // Act & Assert
        Assert.Single(repoConstraint);
        Assert.True(repoConstraint[0].IsGenericType);
        Assert.Equal(typeof(ISimpleRepository<>), repoConstraint[0].GetGenericTypeDefinition());
    }

    #endregion

    #region Inherited CRUD Operations Tests

    [Fact]
    public async Task GetById_WithGuidId_CallsBaseImplementation()
    {
        // Arrange
        var mockRepository = new Mock<ISimpleRepository<TestSimpleEntity>>();
        var mockMapper = new Mock<IMapper>();
        
        var entityId = Guid.NewGuid();
        var entity = new TestSimpleEntity { Id = entityId, Name = "Test", Price = 10.00m };
        var dto = new TestSimpleEntityDto { Id = entityId, Name = "Test", Price = 10.00m };

        mockRepository.Setup(r => r.GetByIdAsync(entityId)).ReturnsAsync(entity);
        mockMapper.Setup(m => m.Map<TestSimpleEntityDto>(entity)).Returns(dto);

        var controller = new TestSimpleController(mockRepository.Object, mockMapper.Object);

        // Act
        var result = await controller.GetById(entityId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedDto = Assert.IsType<TestSimpleEntityDto>(okResult.Value);
        Assert.Equal(entityId, returnedDto.Id);
        Assert.Equal("Test", returnedDto.Name);
        
        mockRepository.Verify(r => r.GetByIdAsync(entityId), Times.Once);
    }

    [Fact]
    public async Task Create_WithGuidBasedEntity_ReturnsCreatedResult()
    {
        // Arrange
        var mockRepository = new Mock<ISimpleRepository<TestSimpleEntity>>();
        var mockMapper = new Mock<IMapper>();

        var createDto = new CreateTestSimpleEntityDto { Name = "New", Price = 15.00m };
        var entity = new TestSimpleEntity { Name = "New", Price = 15.00m };
        var createdId = Guid.NewGuid();
        var createdEntity = new TestSimpleEntity { Id = createdId, Name = "New", Price = 15.00m };
        var dto = new TestSimpleEntityDto { Id = createdId, Name = "New", Price = 15.00m };

        mockMapper.Setup(m => m.Map<TestSimpleEntity>(createDto)).Returns(entity);
        mockRepository.Setup(r => r.AddAsync(entity)).ReturnsAsync(createdEntity);
        mockMapper.Setup(m => m.Map<TestSimpleEntityDto>(createdEntity)).Returns(dto);

        var controller = new TestSimpleController(mockRepository.Object, mockMapper.Object);

        // Act
        var result = await controller.Create(createDto);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(TestSimpleController.GetById), createdResult.ActionName);
        
        var returnedDto = Assert.IsType<TestSimpleEntityDto>(createdResult.Value);
        Assert.Equal(createdId, returnedDto.Id);
        Assert.Equal("New", returnedDto.Name);
    }

    [Fact]
    public async Task Update_WithGuidId_UpdatesEntity()
    {
        // Arrange
        var mockRepository = new Mock<ISimpleRepository<TestSimpleEntity>>();
        var mockMapper = new Mock<IMapper>();

        var entityId = Guid.NewGuid();
        var updateDto = new UpdateTestSimpleEntityDto { Id = entityId, Name = "Updated", Price = 25.00m };
        var existingEntity = new TestSimpleEntity { Id = entityId, Name = "Old", Price = 10.00m };
        var updatedEntity = new TestSimpleEntity { Id = entityId, Name = "Updated", Price = 25.00m };
        var dto = new TestSimpleEntityDto { Id = entityId, Name = "Updated", Price = 25.00m };

        mockRepository.Setup(r => r.GetByIdAsync(entityId)).ReturnsAsync(existingEntity);
        mockMapper.Setup(m => m.Map(updateDto, existingEntity)).Returns(updatedEntity);
        mockRepository.Setup(r => r.UpdateAsync(existingEntity)).ReturnsAsync(updatedEntity);
        mockMapper.Setup(m => m.Map<TestSimpleEntityDto>(updatedEntity)).Returns(dto);

        var controller = new TestSimpleController(mockRepository.Object, mockMapper.Object);

        // Act
        var result = await controller.Update(entityId, updateDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedDto = Assert.IsType<TestSimpleEntityDto>(okResult.Value);
        Assert.Equal("Updated", returnedDto.Name);
        Assert.Equal(25.00m, returnedDto.Price);
    }

    [Fact]
    public async Task Delete_WithGuidId_DeletesEntity()
    {
        // Arrange
        var mockRepository = new Mock<ISimpleRepository<TestSimpleEntity>>();
        var mockMapper = new Mock<IMapper>();

        var entityId = Guid.NewGuid();
        mockRepository.Setup(r => r.DeleteAsync(entityId)).ReturnsAsync(true);

        var controller = new TestSimpleController(mockRepository.Object, mockMapper.Object);

        // Act
        var result = await controller.Delete(entityId);

        // Assert
        Assert.IsType<NoContentResult>(result);
        mockRepository.Verify(r => r.DeleteAsync(entityId), Times.Once);
    }

    [Fact]
    public async Task GetAll_InheritsFromBase_ReturnsPaginatedResults()
    {
        // Arrange
        var mockRepository = new Mock<ISimpleRepository<TestSimpleEntity>>();
        var mockMapper = new Mock<IMapper>();

        var entities = new List<TestSimpleEntity>
        {
            new TestSimpleEntity { Id = Guid.NewGuid(), Name = "Test1", Price = 10.00m },
            new TestSimpleEntity { Id = Guid.NewGuid(), Name = "Test2", Price = 20.00m }
        };

        var dtos = new List<TestSimpleEntityDto>
        {
            new TestSimpleEntityDto { Id = entities[0].Id, Name = "Test1", Price = 10.00m },
            new TestSimpleEntityDto { Id = entities[1].Id, Name = "Test2", Price = 20.00m }
        };

        var pagedResult = new PagedResult<TestSimpleEntity>
        {
            Items = entities,
            TotalCount = 2
        };

        mockRepository.Setup(r => r.GetAllAsync(It.IsAny<QueryOptions<TestSimpleEntity>>()))
            .ReturnsAsync(pagedResult);
        mockMapper.Setup(m => m.Map<List<TestSimpleEntityDto>>(entities)).Returns(dtos);

        var controller = new TestSimpleController(mockRepository.Object, mockMapper.Object);

        // Act
        var result = await controller.GetAll();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedResult = Assert.IsType<PagedResult<TestSimpleEntityDto>>(okResult.Value);
        Assert.Equal(2, returnedResult.TotalCount);
        Assert.Equal(2, returnedResult.Items.Count());
    }

    #endregion

    #region Type Constraints Tests

    [Fact]
    public void SimpleApiControllerBase_HasCorrectGenericParameterCount()
    {
        // Arrange
        var controllerType = typeof(SimpleApiControllerBase<,,,,>);

        // Act
        var genericParameters = controllerType.GetGenericArguments();

        // Assert
        Assert.Equal(5, genericParameters.Length); // TEntity, TDto, TCreateDto, TUpdateDto, TRepository
    }

    [Fact]
    public void SimpleApiControllerBase_RequiresReferenceTypeEntity()
    {
        // Arrange
        var controllerType = typeof(SimpleApiControllerBase<,,,,>);
        var entityParameter = controllerType.GetGenericArguments()[0];

        // Act
        var isReferenceType = (entityParameter.GenericParameterAttributes & System.Reflection.GenericParameterAttributes.ReferenceTypeConstraint) != 0;

        // Assert
        Assert.True(isReferenceType);
    }

    #endregion

    #region Design Pattern Tests

    [Fact]
    public void SimpleApiControllerBase_SimplifiesSignature_ComparedToAdvanced()
    {
        // Arrange
        var simpleType = typeof(SimpleApiControllerBase<,,,,>);
        var advancedType = typeof(AdvancedApiControllerBase<,,,,,>);

        // Act
        var simpleParamCount = simpleType.GetGenericArguments().Length;
        var advancedParamCount = advancedType.GetGenericArguments().Length;

        // Assert
        Assert.Equal(5, simpleParamCount); // Simple has 5 parameters
        Assert.Equal(6, advancedParamCount); // Advanced has 6 parameters (includes TKey)
        Assert.True(simpleParamCount < advancedParamCount, 
            "SimpleApiControllerBase should have fewer generic parameters than AdvancedApiControllerBase");
    }

    [Fact]
    public void SimpleApiControllerBase_IsAbstract()
    {
        // Arrange
        var controllerType = typeof(SimpleApiControllerBase<,,,,>);

        // Act
        var isAbstract = controllerType.IsAbstract;

        // Assert
        Assert.True(isAbstract, "SimpleApiControllerBase should be abstract");
    }

    [Fact]
    public void SimpleApiControllerBase_HasApiControllerAttribute()
    {
        // Arrange
        var controllerType = typeof(TestSimpleController);

        // Act
        var hasAttribute = controllerType.GetCustomAttributes(typeof(ApiControllerAttribute), true).Any();

        // Assert
        Assert.True(hasAttribute, "SimpleApiControllerBase should have ApiController attribute");
    }

    [Fact]
    public void SimpleApiControllerBase_HasRouteAttribute()
    {
        // Arrange
        var controllerType = typeof(TestSimpleController);

        // Act
        var routeAttribute = controllerType.GetCustomAttributes(typeof(RouteAttribute), true)
            .FirstOrDefault() as RouteAttribute;

        // Assert
        Assert.NotNull(routeAttribute);
        Assert.Equal("api/[controller]", routeAttribute!.Template);
    }

    #endregion
}
