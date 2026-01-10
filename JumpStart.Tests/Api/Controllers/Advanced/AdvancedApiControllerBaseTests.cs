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
using System.Threading.Tasks;
using AutoMapper;
using JumpStart.Api.Controllers.Advanced;
using JumpStart.Api.DTOs;
using JumpStart.Api.DTOs.Advanced;
using JumpStart.Data.Advanced;
using JumpStart.Repositories;
using JumpStart.Repositories.Advanced;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace JumpStart.Tests.Api.Controllers.Advanced;

/// <summary>
/// Unit tests for the <see cref="AdvancedApiControllerBase{TEntity, TKey, TDto, TCreateDto, TUpdateDto, TRepository}"/> class.
/// Tests all CRUD operations, validation, error handling, and AutoMapper integration.
/// </summary>
public class AdvancedApiControllerBaseTests
{
    #region Test Classes

    /// <summary>
    /// Test entity for controller testing.
    /// </summary>
    public class TestEntity : IEntity<int>
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }

    /// <summary>
    /// Test DTO for read operations.
    /// </summary>
    public class TestEntityDto : EntityDto<int>
    {
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }

    /// <summary>
    /// Test DTO for create operations.
    /// </summary>
    public class CreateTestEntityDto : ICreateDto
    {
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }

    /// <summary>
    /// Test DTO for update operations.
    /// </summary>
    public class UpdateTestEntityDto : IUpdateDto<int>
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }

    /// <summary>
    /// Concrete test controller for testing base controller functionality.
    /// </summary>
    public class TestController : AdvancedApiControllerBase<
        TestEntity,
        int,
        TestEntityDto,
        CreateTestEntityDto,
        UpdateTestEntityDto,
        IRepository<TestEntity, int>>
    {
        public TestController(IRepository<TestEntity, int> repository, IMapper mapper)
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
        var mockRepository = new Mock<IRepository<TestEntity, int>>();
        var mockMapper = new Mock<IMapper>();

        // Act
        var controller = new TestController(mockRepository.Object, mockMapper.Object);

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
            new TestController(null!, mockMapper.Object));
        Assert.Equal("repository", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullMapper_ThrowsArgumentNullException()
    {
        // Arrange
        var mockRepository = new Mock<IRepository<TestEntity, int>>();

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new TestController(mockRepository.Object, null!));
        Assert.Equal("mapper", exception.ParamName);
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetById_WithExistingEntity_ReturnsOkWithDto()
    {
        // Arrange
        var mockRepository = new Mock<IRepository<TestEntity, int>>();
        var mockMapper = new Mock<IMapper>();
        
        var entity = new TestEntity { Id = 1, Name = "Test", Price = 10.00m };
        var dto = new TestEntityDto { Id = 1, Name = "Test", Price = 10.00m };

        mockRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(entity);
        mockMapper.Setup(m => m.Map<TestEntityDto>(entity)).Returns(dto);

        var controller = new TestController(mockRepository.Object, mockMapper.Object);

        // Act
        var result = await controller.GetById(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedDto = Assert.IsType<TestEntityDto>(okResult.Value);
        Assert.Equal(1, returnedDto.Id);
        Assert.Equal("Test", returnedDto.Name);
        Assert.Equal(10.00m, returnedDto.Price);
        
        mockRepository.Verify(r => r.GetByIdAsync(1), Times.Once);
        mockMapper.Verify(m => m.Map<TestEntityDto>(entity), Times.Once);
    }

    [Fact]
    public async Task GetById_WithNonExistentEntity_ReturnsNotFound()
    {
        // Arrange
        var mockRepository = new Mock<IRepository<TestEntity, int>>();
        var mockMapper = new Mock<IMapper>();

        mockRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((TestEntity?)null);

        var controller = new TestController(mockRepository.Object, mockMapper.Object);

        // Act
        var result = await controller.GetById(999);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
        mockRepository.Verify(r => r.GetByIdAsync(999), Times.Once);
        mockMapper.Verify(m => m.Map<TestEntityDto>(It.IsAny<TestEntity>()), Times.Never);
    }

    #endregion

    #region GetAll Tests

    [Fact]
    public async Task GetAll_WithoutPagination_ReturnsAllEntities()
    {
        // Arrange
        var mockRepository = new Mock<IRepository<TestEntity, int>>();
        var mockMapper = new Mock<IMapper>();

        var entities = new List<TestEntity>
        {
            new TestEntity { Id = 1, Name = "Test1", Price = 10.00m },
            new TestEntity { Id = 2, Name = "Test2", Price = 20.00m }
        };

        var dtos = new List<TestEntityDto>
        {
            new TestEntityDto { Id = 1, Name = "Test1", Price = 10.00m },
            new TestEntityDto { Id = 2, Name = "Test2", Price = 20.00m }
        };

        var pagedResult = new PagedResult<TestEntity>
        {
            Items = entities,
            TotalCount = 2
        };

        mockRepository.Setup(r => r.GetAllAsync(It.IsAny<QueryOptions<TestEntity>>()))
            .ReturnsAsync(pagedResult);
        mockMapper.Setup(m => m.Map<List<TestEntityDto>>(entities)).Returns(dtos);

        var controller = new TestController(mockRepository.Object, mockMapper.Object);

        // Act
        var result = await controller.GetAll();

        // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedResult = Assert.IsType<PagedResult<TestEntityDto>>(okResult.Value);
            Assert.Equal(2, returnedResult.TotalCount);
            Assert.Equal(2, returnedResult.Items.Count());
            mockRepository.Verify(r => r.GetAllAsync(It.IsAny<QueryOptions<TestEntity>>()), Times.Once);
        }

    [Fact]
    public async Task GetAll_WithPagination_ReturnsPagedEntities()
    {
        // Arrange
        var mockRepository = new Mock<IRepository<TestEntity, int>>();
        var mockMapper = new Mock<IMapper>();

        var entities = new List<TestEntity>
        {
            new TestEntity { Id = 1, Name = "Test1", Price = 10.00m }
        };

        var dtos = new List<TestEntityDto>
        {
            new TestEntityDto { Id = 1, Name = "Test1", Price = 10.00m }
        };

        var pagedResult = new PagedResult<TestEntity>
        {
            Items = entities,
            TotalCount = 100,
            PageNumber = 1,
            PageSize = 1
        };

        mockRepository.Setup(r => r.GetAllAsync(It.IsAny<QueryOptions<TestEntity>>()))
            .ReturnsAsync(pagedResult);
        mockMapper.Setup(m => m.Map<List<TestEntityDto>>(entities)).Returns(dtos);

        var controller = new TestController(mockRepository.Object, mockMapper.Object);

        // Act
        var result = await controller.GetAll(pageNumber: 1, pageSize: 1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedResult = Assert.IsType<PagedResult<TestEntityDto>>(okResult.Value);
        Assert.Equal(100, returnedResult.TotalCount);
        Assert.Equal(1, returnedResult.PageNumber);
        Assert.Equal(1, returnedResult.PageSize);
        Assert.Single(returnedResult.Items);
    }

    [Fact]
    public async Task GetAll_WithSortDescending_PassesCorrectOptions()
    {
        // Arrange
        var mockRepository = new Mock<IRepository<TestEntity, int>>();
        var mockMapper = new Mock<IMapper>();

        var pagedResult = new PagedResult<TestEntity>
        {
            Items = new List<TestEntity>(),
            TotalCount = 0
        };

        QueryOptions<TestEntity>? capturedOptions = null;
        mockRepository.Setup(r => r.GetAllAsync(It.IsAny<QueryOptions<TestEntity>>()))
            .Callback<QueryOptions<TestEntity>>(opts => capturedOptions = opts)
            .ReturnsAsync(pagedResult);
        mockMapper.Setup(m => m.Map<List<TestEntityDto>>(It.IsAny<List<TestEntity>>()))
            .Returns(new List<TestEntityDto>());

        var controller = new TestController(mockRepository.Object, mockMapper.Object);

        // Act
        await controller.GetAll(sortDescending: true);

        // Assert
        Assert.NotNull(capturedOptions);
        Assert.True(capturedOptions!.SortDescending);
    }

    #endregion

    #region Create Tests

    [Fact]
    public async Task Create_WithValidDto_ReturnsCreatedAtAction()
    {
        // Arrange
        var mockRepository = new Mock<IRepository<TestEntity, int>>();
        var mockMapper = new Mock<IMapper>();

        var createDto = new CreateTestEntityDto { Name = "New", Price = 15.00m };
        var entity = new TestEntity { Name = "New", Price = 15.00m };
        var createdEntity = new TestEntity { Id = 1, Name = "New", Price = 15.00m };
        var dto = new TestEntityDto { Id = 1, Name = "New", Price = 15.00m };

        mockMapper.Setup(m => m.Map<TestEntity>(createDto)).Returns(entity);
        mockRepository.Setup(r => r.AddAsync(entity)).ReturnsAsync(createdEntity);
        mockMapper.Setup(m => m.Map<TestEntityDto>(createdEntity)).Returns(dto);

        var controller = new TestController(mockRepository.Object, mockMapper.Object);

        // Act
        var result = await controller.Create(createDto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(nameof(TestController.GetById), createdResult.ActionName);
            Assert.NotNull(createdResult.RouteValues);
            Assert.True(createdResult.RouteValues!.ContainsKey("id"));
            Assert.Equal(1, createdResult.RouteValues["id"]);

            var returnedDto = Assert.IsType<TestEntityDto>(createdResult.Value);
            Assert.Equal(1, returnedDto.Id);
            Assert.Equal("New", returnedDto.Name);

            mockRepository.Verify(r => r.AddAsync(entity), Times.Once);
        }

    [Fact]
    public async Task Create_WithInvalidModelState_ReturnsBadRequest()
    {
        // Arrange
        var mockRepository = new Mock<IRepository<TestEntity, int>>();
        var mockMapper = new Mock<IMapper>();
        var controller = new TestController(mockRepository.Object, mockMapper.Object);
        
        controller.ModelState.AddModelError("Name", "Required");

        var createDto = new CreateTestEntityDto();

        // Act
        var result = await controller.Create(createDto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
        mockRepository.Verify(r => r.AddAsync(It.IsAny<TestEntity>()), Times.Never);
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_WithValidDto_ReturnsOkWithUpdatedDto()
    {
        // Arrange
        var mockRepository = new Mock<IRepository<TestEntity, int>>();
        var mockMapper = new Mock<IMapper>();

        var updateDto = new UpdateTestEntityDto { Id = 1, Name = "Updated", Price = 25.00m };
        var existingEntity = new TestEntity { Id = 1, Name = "Old", Price = 10.00m };
        var updatedEntity = new TestEntity { Id = 1, Name = "Updated", Price = 25.00m };
        var dto = new TestEntityDto { Id = 1, Name = "Updated", Price = 25.00m };

        mockRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingEntity);
        mockMapper.Setup(m => m.Map(updateDto, existingEntity)).Returns(updatedEntity);
        mockRepository.Setup(r => r.UpdateAsync(existingEntity)).ReturnsAsync(updatedEntity);
        mockMapper.Setup(m => m.Map<TestEntityDto>(updatedEntity)).Returns(dto);

        var controller = new TestController(mockRepository.Object, mockMapper.Object);

        // Act
        var result = await controller.Update(1, updateDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedDto = Assert.IsType<TestEntityDto>(okResult.Value);
        Assert.Equal("Updated", returnedDto.Name);
        Assert.Equal(25.00m, returnedDto.Price);

        mockRepository.Verify(r => r.UpdateAsync(existingEntity), Times.Once);
    }

    [Fact]
    public async Task Update_WithNonExistentEntity_ReturnsNotFound()
    {
        // Arrange
        var mockRepository = new Mock<IRepository<TestEntity, int>>();
        var mockMapper = new Mock<IMapper>();

        var updateDto = new UpdateTestEntityDto { Id = 999, Name = "Updated", Price = 25.00m };

        mockRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((TestEntity?)null);

        var controller = new TestController(mockRepository.Object, mockMapper.Object);

        // Act
        var result = await controller.Update(999, updateDto);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
        mockRepository.Verify(r => r.UpdateAsync(It.IsAny<TestEntity>()), Times.Never);
    }

    [Fact]
    public async Task Update_WithIdMismatch_ReturnsBadRequest()
    {
        // Arrange
        var mockRepository = new Mock<IRepository<TestEntity, int>>();
        var mockMapper = new Mock<IMapper>();

        var updateDto = new UpdateTestEntityDto { Id = 2, Name = "Updated", Price = 25.00m };

        var controller = new TestController(mockRepository.Object, mockMapper.Object);

        // Act
        var result = await controller.Update(1, updateDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("ID mismatch", badRequestResult.Value);
        
        mockRepository.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Never);
        mockRepository.Verify(r => r.UpdateAsync(It.IsAny<TestEntity>()), Times.Never);
    }

    [Fact]
    public async Task Update_WithInvalidModelState_ReturnsBadRequest()
    {
        // Arrange
        var mockRepository = new Mock<IRepository<TestEntity, int>>();
        var mockMapper = new Mock<IMapper>();
        var controller = new TestController(mockRepository.Object, mockMapper.Object);
        
        controller.ModelState.AddModelError("Name", "Required");

        var updateDto = new UpdateTestEntityDto { Id = 1 };

        // Act
        var result = await controller.Update(1, updateDto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
        mockRepository.Verify(r => r.UpdateAsync(It.IsAny<TestEntity>()), Times.Never);
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task Delete_WithExistingEntity_ReturnsNoContent()
    {
        // Arrange
        var mockRepository = new Mock<IRepository<TestEntity, int>>();
        var mockMapper = new Mock<IMapper>();

        mockRepository.Setup(r => r.DeleteAsync(1)).ReturnsAsync(true);

        var controller = new TestController(mockRepository.Object, mockMapper.Object);

        // Act
        var result = await controller.Delete(1);

        // Assert
        Assert.IsType<NoContentResult>(result);
        mockRepository.Verify(r => r.DeleteAsync(1), Times.Once);
    }

    [Fact]
    public async Task Delete_WithNonExistentEntity_ReturnsNotFound()
    {
        // Arrange
        var mockRepository = new Mock<IRepository<TestEntity, int>>();
        var mockMapper = new Mock<IMapper>();

        mockRepository.Setup(r => r.DeleteAsync(999)).ReturnsAsync(false);

        var controller = new TestController(mockRepository.Object, mockMapper.Object);

        // Act
        var result = await controller.Delete(999);

        // Assert
        Assert.IsType<NotFoundResult>(result);
        mockRepository.Verify(r => r.DeleteAsync(999), Times.Once);
    }

    #endregion
}
