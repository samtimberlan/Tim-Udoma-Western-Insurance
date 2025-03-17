namespace Tim_Udoma_Western_Insurance.Tests.Services
{
    using global::Tim_Udoma_Western_Insurance.Data.Models;
    using global::Tim_Udoma_Western_Insurance.DTOs.Responses;
    using global::Tim_Udoma_Western_Insurance.DTOs.Responses.Http;
    using global::Tim_Udoma_Western_Insurance.Services;
    using global::Tim_Udoma_Western_Insurance.Services.Interfaces;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Xunit;

    public class ProductServiceTests
    {
        private readonly Mock<DbSet<Data.Models.Product>> _mockProductDbSet;
        private readonly Mock<INotificationService> _mockNotificationService;
        private readonly Mock<ILogger<BaseService>> _mockLogger;
        private readonly WIShopDBContext _dbContext;
        private readonly ProductService _productService;
        private readonly List<Data.Models.Product> _productData;

        public ProductServiceTests()
        {
            // Create the in-memory database
            var options = new DbContextOptionsBuilder<WIShopDBContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new WIShopDBContext(options);

            // Initialize data collection
            _productData = new List<Data.Models.Product>();

            // Set up mock DbSet
            _mockProductDbSet = new Mock<DbSet<Data.Models.Product>>();
            SetupMockDbSet(_productData, _mockProductDbSet);

            // Set up other mocks
            _mockNotificationService = new Mock<INotificationService>();
            _mockLogger = new Mock<ILogger<BaseService>>();

            // Initialize service with actual DbContext and mocked dependencies
            _productService = new ProductService(
                _dbContext,
                _mockNotificationService.Object,
                _mockLogger.Object
            );
        }

        #region AddAsync Tests

        [Fact]
        public async Task AddAsync_WhenValidProduct_ReturnsSuccessResult()
        {
            // Arrange
            var productDto = new DTOs.Requests.Product
            {
                Sku = "TEST-SKU-123",
                Title = "Test Product",
                Description = "Test Description"
            };

            // Act
            var result = await _productService.AddAsync(productDto);

            // Assert
            Assert.IsType<SuccessResult>(result);
            Assert.Equal("Product added successfully", result.Message);

            // Verify product was added to database
            var addedProduct = await _dbContext.Products.FirstOrDefaultAsync(p => p.Sku == productDto.Sku);
            Assert.NotNull(addedProduct);
            Assert.Equal(productDto.Title, addedProduct.Title);
        }

        [Fact]
        public async Task AddAsync_WhenSkuExists_ReturnsConflictError()
        {
            // Arrange
            var existingSku = "EXISTING-SKU";

            // Add existing product directly to context
            _dbContext.Products.Add(new Data.Models.Product
            {
                Sku = existingSku,
                Title = "Existing Product",
                Description = "Existing Description",
                Active = true,
                DateCreated = DateTime.Now,
                Reference = Guid.NewGuid()
            });
            await _dbContext.SaveChangesAsync();

            var productDto = new DTOs.Requests.Product
            {
                Sku = existingSku,
                Title = "Test Product",
                Description = "Test Description"
            };

            // Act
            var result = await _productService.AddAsync(productDto);

            // Assert
            Assert.IsType<ErrorResult>(result);
            var errorResult = (ErrorResult)result;
            Assert.Equal("Product with SKU already exists", errorResult.Message);
            Assert.Equal(StatusCodes.Status409Conflict, errorResult.Status);
        }

        #endregion

        #region GetAllAsync Tests

        [Fact]
        public async Task GetAllAsync_WhenProductsExist_ReturnsPaginatedList()
        {
            // Arrange
            var searchTerm = "TEST";
            var pageNumber = 1;
            var pageSize = 10;

            // Add test products directly to context
            _dbContext.Products.AddRange(new List<Data.Models.Product>
            {
                new Data.Models.Product {
                    Sku = "TEST-001",
                    Title = "TEST PRODUCT 1",
                    Description = "DESCRIPTION 1",
                    Active = true,
                    DateCreated = DateTime.Now,
                    Reference = Guid.NewGuid()
                },
                new Data.Models.Product {
                    Sku = "TEST-002",
                    Title = "TEST PRODUCT 2",
                    Description = "DESCRIPTION 2",
                    Active = true,
                    DateCreated = DateTime.Now,
                    Reference = Guid.NewGuid()
                }
            });
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _productService.GetAllAsync(searchTerm, pageNumber, pageSize);

            // Assert
            Assert.IsType<SuccessResult>(result);
            var successResult = (SuccessResult)result;
            Assert.IsType<PagedList<ProductResponse>>(successResult.Content);

            var pagedList = (PagedList<ProductResponse>)successResult.Content;
            Assert.Equal(2, pagedList.Count);
        }

        [Fact]
        public async Task GetAllAsync_WhenNoProductsMatch_ReturnsEmptyList()
        {
            // Arrange
            var searchTerm = "NONEXISTENT";
            var pageNumber = 1;
            var pageSize = 10;

            // Add test products directly to context
            _dbContext.Products.Add(new Data.Models.Product
            {
                Sku = "TEST-001",
                Title = "TEST PRODUCT 1",
                Description = "DESCRIPTION 1",
                Active = true,
                DateCreated = DateTime.Now,
                Reference = Guid.NewGuid()
            });
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _productService.GetAllAsync(searchTerm, pageNumber, pageSize);

            // Assert
            Assert.IsType<SuccessResult>(result);
            var successResult = (SuccessResult)result;
            Assert.IsType<PagedList<ProductResponse>>(successResult.Content);
            var paginatedList = (PagedList<ProductResponse>)successResult.Content;
            Assert.Empty(paginatedList);
        }

        #endregion

        #region Get Tests

        [Fact]
        public async Task Get_WhenProductExists_ReturnsProduct()
        {
            // Arrange
            var sku = "TEST-001";
            var product = new Data.Models.Product
            {
                Sku = sku,
                Title = "TEST PRODUCT",
                Description = "TEST DESCRIPTION",
                Active = true,
                DateCreated = DateTime.Now,
                Reference = Guid.NewGuid()
            };

            _dbContext.Products.Add(product);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _productService.Get(sku);

            // Assert
            Assert.IsType<SuccessResult>(result);
            var successResult = (SuccessResult)result;
            Assert.IsType<ProductResponse>(successResult.Content);
            var productResponse = (ProductResponse)successResult.Content;
            Assert.Equal(sku, productResponse.SKU);
            Assert.Equal(product.Title, productResponse.Title);
        }

        [Fact]
        public async Task Get_WhenProductNotFound_ReturnsNotFoundError()
        {
            // Arrange
            var sku = "NONEXISTENT-SKU";

            // Act
            var result = await _productService.Get(sku);

            // Assert
            Assert.IsType<ErrorResult>(result);
            var errorResult = (ErrorResult)result;
            Assert.Equal("Product not found", errorResult.Message);
            Assert.Equal(StatusCodes.Status404NotFound, errorResult.Status);
        }

        [Fact]
        public async Task Get_WhenInactiveProductAndOnlyActiveTrue_ReturnsNotFoundError()
        {
            // Arrange
            var sku = "TEST-001";
            var product = new Data.Models.Product
            {
                Sku = sku,
                Title = "TEST PRODUCT",
                Description = "TEST DESCRIPTION",
                Active = false,
                DateCreated = DateTime.Now,
                Reference = Guid.NewGuid()
            };

            _dbContext.Products.Add(product);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _productService.Get(sku, onlyActive: true);

            // Assert
            Assert.IsType<ErrorResult>(result);
            var errorResult = (ErrorResult)result;
            Assert.Equal("Product not found", errorResult.Message);
            Assert.Equal(StatusCodes.Status404NotFound, errorResult.Status);
        }

        #endregion

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_WhenProductExists_ReturnsSuccessResult()
        {
            // Arrange
            var sku = "TEST-001";
            var product = new Data.Models.Product
            {
                Sku = sku,
                Title = "TEST PRODUCT",
                Description = "TEST DESCRIPTION",
                Active = true,
                DateCreated = DateTime.Now,
                Reference = Guid.NewGuid()
            };

            _dbContext.Products.Add(product);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _productService.DeleteAsync(sku);

            // Assert
            Assert.IsType<SuccessResult>(result);
            Assert.Equal("Product deleted successfully", result.Message);

            // Verify product was deleted
            var deletedProduct = await _dbContext.Products.FirstOrDefaultAsync(p => p.Sku == sku);
            Assert.Null(deletedProduct);
        }

        [Fact]
        public async Task DeleteAsync_WhenProductNotFound_ReturnsNotFoundError()
        {
            // Arrange
            var sku = "NONEXISTENT-SKU";

            // Act
            var result = await _productService.DeleteAsync(sku);

            // Assert
            Assert.IsType<ErrorResult>(result);
            var errorResult = (ErrorResult)result;
            Assert.Equal("Product not found", errorResult.Message);
            Assert.Equal(StatusCodes.Status404NotFound, errorResult.Status);
        }

        #endregion

        #region Deactivate Tests

        [Fact]
        public async Task Deactivate_WhenProductExists_DeactivatesProductAndNotifiesUsers()
        {
            // Arrange
            var sku = "TEST-001";
            var product = new Data.Models.Product
            {
                Sku = sku,
                Title = "TEST PRODUCT",
                Description = "TEST DESCRIPTION",
                Active = true,
                DateCreated = DateTime.Now,
                Reference = Guid.NewGuid()
            };

            _dbContext.Products.Add(product);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _productService.Deactivate(sku);

            // Assert
            Assert.IsType<SuccessResult>(result);
            Assert.Equal("Product deactivated successfully", result.Message);

            // Verify product was deactivated
            var deactivatedProduct = await _dbContext.Products.FirstOrDefaultAsync(p => p.Sku == sku);
            Assert.NotNull(deactivatedProduct);
            Assert.False(deactivatedProduct.Active);

            _mockNotificationService.Verify(ns => ns.Notify("1", "Product Deactivated"), Times.Once);
            _mockNotificationService.Verify(ns => ns.Notify("2", "Product Deactivated"), Times.Once);
        }

        [Fact]
        public async Task Deactivate_WhenProductNotFound_ReturnsNotFoundError()
        {
            // Arrange
            var sku = "NONEXISTENT-SKU";

            // Act
            var result = await _productService.Deactivate(sku);

            // Assert
            Assert.IsType<ErrorResult>(result);
            var errorResult = (ErrorResult)result;
            Assert.Equal("Product not found", errorResult.Message);
            Assert.Equal(StatusCodes.Status404NotFound, errorResult.Status);
            _mockNotificationService.Verify(ns => ns.Notify(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        #endregion

        #region Helper Methods

        private static void SetupMockDbSet<T>(List<T> data, Mock<DbSet<T>> mockDbSet) where T : class
        {
            var queryable = data.AsQueryable();
            mockDbSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
            mockDbSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockDbSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockDbSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());

            mockDbSet.Setup(m => m.Add(It.IsAny<T>())).Callback<T>(data.Add);
            mockDbSet.Setup(m => m.Remove(It.IsAny<T>())).Callback<T>(item => data.Remove(item));
        }

        #endregion
    }
}