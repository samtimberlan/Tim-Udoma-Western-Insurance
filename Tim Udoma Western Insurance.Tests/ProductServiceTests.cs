namespace Tim_Udoma_Western_Insurance.Tests
{
    using global::Tim_Udoma_Western_Insurance.Data.Models;
    using global::Tim_Udoma_Western_Insurance.DTOs.Responses;
    using global::Tim_Udoma_Western_Insurance.DTOs.Responses.Http;
    using global::Tim_Udoma_Western_Insurance.Services;
    using global::Tim_Udoma_Western_Insurance.Services.Interfaces;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Query;
    using Microsoft.Extensions.Logging;
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using Xunit;

    namespace Tim_Udoma_Western_Insurance.Tests.Services
    {
        public class ProductServiceTests
        {
            private readonly Mock<WIShopDBContext> _mockDbContext;
            private readonly Mock<INotificationService> _mockNotificationService;
            private readonly Mock<ILogger<BaseService>> _mockLogger;
            private readonly ProductService _productService;
            private readonly Mock<DbSet<Data.Models.Product>> _mockProductDbSet;

            public ProductServiceTests()
            {
                // Setup mocks
                _mockDbContext = new Mock<WIShopDBContext>();
                _mockNotificationService = new Mock<INotificationService>();
                _mockLogger = new Mock<ILogger<BaseService>>();
                _mockProductDbSet = new Mock<DbSet<Data.Models.Product>>();

                // Initialize service with mocks
                _productService = new ProductService(
                    _mockDbContext.Object,
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

                var products = new List<Data.Models.Product>();
                SetupMockDbSet(products, _mockProductDbSet);

                _mockDbContext.Setup(db => db.Products).Returns(_mockProductDbSet.Object);
                _mockDbContext.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()))
                    .Callback(() => products.Add(It.IsAny<Data.Models.Product>()))
                    .ReturnsAsync(1);

                // Act
                var result = await _productService.AddAsync(productDto);

                // Assert
                Assert.IsType<SuccessResult>(result);
                Assert.Equal("Product added successfully", result.Message);
                _mockDbContext.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            }

            [Fact]
            public async Task AddAsync_WhenSkuExists_ReturnsConflictError()
            {
                // Arrange
                var existingSku = "EXISTING-SKU";
                var productDto = new DTOs.Requests.Product
                {
                    Sku = existingSku,
                    Title = "Test Product",
                    Description = "Test Description"
                };

                var existingProducts = new List<Data.Models.Product>
            {
                new Data.Models.Product { Sku = existingSku }
            };

                SetupMockDbSet(existingProducts, _mockProductDbSet);
                _mockDbContext.Setup(db => db.Products).Returns(_mockProductDbSet.Object);

                // Act
                var result = await _productService.AddAsync(productDto);

                // Assert
                Assert.IsType<ErrorResult>(result);
                var errorResult = (ErrorResult)result;
                Assert.Equal("Product with SKU already exists", errorResult.Message);
                Assert.Equal(StatusCodes.Status409Conflict, errorResult.Status);
                _mockDbContext.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
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

                var products = new List<Data.Models.Product>
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
            };

                var mockQueryable = products.AsQueryable().BuildMockDbSet();
                _mockDbContext.Setup(db => db.Products).Returns(mockQueryable.Object);

                // Act
                var result = await _productService.GetAllAsync(searchTerm, pageNumber, pageSize);

                // Assert
                Assert.IsType<SuccessResult>(result);
                var successResult = (SuccessResult)result;
                Assert.IsType<PagedList<ProductResponse>>(successResult.Content);
            }

            [Fact]
            public async Task GetAllAsync_WhenNoProductsMatch_ReturnsEmptyList()
            {
                // Arrange
                var searchTerm = "NONEXISTENT";
                var pageNumber = 1;
                var pageSize = 10;

                var products = new List<Data.Models.Product>
            {
                new Data.Models.Product {
                    Sku = "TEST-001",
                    Title = "TEST PRODUCT 1",
                    Description = "DESCRIPTION 1",
                    Active = true
                }
            };

                var mockQueryable = products.AsQueryable().BuildMockDbSet();
                _mockDbContext.Setup(db => db.Products).Returns(mockQueryable.Object);

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

                var products = new List<Data.Models.Product> { product };
                SetupMockDbSet(products, _mockProductDbSet);
                _mockDbContext.Setup(db => db.Products).Returns(_mockProductDbSet.Object);

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
                var products = new List<Data.Models.Product>();

                SetupMockDbSet(products, _mockProductDbSet);
                _mockDbContext.Setup(db => db.Products).Returns(_mockProductDbSet.Object);

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

                var products = new List<Data.Models.Product> { product };
                SetupMockDbSet(products, _mockProductDbSet);
                _mockDbContext.Setup(db => db.Products).Returns(_mockProductDbSet.Object);

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
                    Description = "TEST DESCRIPTION"
                };

                var products = new List<Data.Models.Product> { product };
                SetupMockDbSet(products, _mockProductDbSet);
                _mockDbContext.Setup(db => db.Products).Returns(_mockProductDbSet.Object);
                _mockDbContext.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()))
                    .Callback(() => products.Remove(product))
                    .ReturnsAsync(1);

                // Act
                var result = await _productService.DeleteAsync(sku);

                // Assert
                Assert.IsType<SuccessResult>(result);
                Assert.Equal("Product deleted successfully", result.Message);
                _mockDbContext.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            }

            [Fact]
            public async Task DeleteAsync_WhenProductNotFound_ReturnsNotFoundError()
            {
                // Arrange
                var sku = "NONEXISTENT-SKU";
                var products = new List<Data.Models.Product>();

                SetupMockDbSet(products, _mockProductDbSet);
                _mockDbContext.Setup(db => db.Products).Returns(_mockProductDbSet.Object);

                // Act
                var result = await _productService.DeleteAsync(sku);

                // Assert
                Assert.IsType<ErrorResult>(result);
                var errorResult = (ErrorResult)result;
                Assert.Equal("Product not found", errorResult.Message);
                Assert.Equal(StatusCodes.Status404NotFound, errorResult.Status);
                _mockDbContext.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
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
                    Active = true
                };

                var products = new List<Data.Models.Product> { product };
                SetupMockDbSet(products, _mockProductDbSet);
                _mockDbContext.Setup(db => db.Products).Returns(_mockProductDbSet.Object);
                _mockDbContext.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()))
                    .Callback(() => product.Active = false)
                    .ReturnsAsync(1);

                // Act
                var result = await _productService.Deactivate(sku);

                // Assert
                Assert.IsType<SuccessResult>(result);
                Assert.Equal("Product deactivated successfully", result.Message);
                Assert.False(product.Active);
                _mockDbContext.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
                _mockNotificationService.Verify(ns => ns.Notify("1", "Product Deactivated"), Times.Once);
                _mockNotificationService.Verify(ns => ns.Notify("2", "Product Deactivated"), Times.Once);
            }

            [Fact]
            public async Task Deactivate_WhenProductNotFound_ReturnsNotFoundError()
            {
                // Arrange
                var sku = "NONEXISTENT-SKU";
                var products = new List<Data.Models.Product>();

                SetupMockDbSet(products, _mockProductDbSet);
                _mockDbContext.Setup(db => db.Products).Returns(_mockProductDbSet.Object);

                // Act
                var result = await _productService.Deactivate(sku);

                // Assert
                Assert.IsType<ErrorResult>(result);
                var errorResult = (ErrorResult)result;
                Assert.Equal("Product not found", errorResult.Message);
                Assert.Equal(StatusCodes.Status404NotFound, errorResult.Status);
                _mockDbContext.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
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

        // Extension method to build mock DbSet from queryable
        public static class MockDbSetExtensions
        {
            public static Mock<DbSet<T>> BuildMockDbSet<T>(this IQueryable<T> source) where T : class
            {
                var mock = new Mock<DbSet<T>>();
                mock.As<IQueryable<T>>().Setup(m => m.Provider).Returns(source.Provider);
                mock.As<IQueryable<T>>().Setup(m => m.Expression).Returns(source.Expression);
                mock.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(source.ElementType);
                mock.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => source.GetEnumerator());
                mock.As<IAsyncEnumerable<T>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                    .Returns(new TestAsyncEnumerator<T>(source.GetEnumerator()));
                mock.As<IQueryable<T>>().Setup(m => m.Provider)
                    .Returns(new TestAsyncQueryProvider<T>(source.Provider));

                return mock;
            }
        }
    }

    // Helper classes for async queryable mocking
    internal class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
    {
        private readonly IQueryProvider _inner;

        internal TestAsyncQueryProvider(IQueryProvider inner)
        {
            _inner = inner;
        }

        public IQueryable CreateQuery(Expression expression)
        {
            return new TestAsyncEnumerable<TEntity>(expression);
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new TestAsyncEnumerable<TElement>(expression);
        }

        public object Execute(Expression expression)
        {
            return _inner.Execute(expression);
        }

        public TResult Execute<TResult>(Expression expression)
        {
            return _inner.Execute<TResult>(expression);
        }

        public IAsyncEnumerable<TResult> ExecuteAsync<TResult>(Expression expression)
        {
            return new TestAsyncEnumerable<TResult>(expression);
        }

        public Task<TResult> ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
        {
            return Task.FromResult(Execute<TResult>(expression));
        }

        TResult IAsyncQueryProvider.ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }

    internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
    {
        private readonly IQueryProvider _provider;

        public TestAsyncEnumerable(IEnumerable<T> enumerable)
            : base(enumerable)
        {
            _provider = new TestAsyncQueryProvider<T>(this.AsQueryable().Provider);
        }

        public TestAsyncEnumerable(Expression expression)
            : base(expression)
        {
            _provider = new TestAsyncQueryProvider<T>(this.AsQueryable().Provider);
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
        }

        IQueryProvider IQueryable.Provider => _provider;
    }

    internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _inner;

        public TestAsyncEnumerator(IEnumerator<T> inner)
        {
            _inner = inner;
        }

        public ValueTask<bool> MoveNextAsync()
        {
            return new ValueTask<bool>(_inner.MoveNext());
        }

        public T Current => _inner.Current;

        public ValueTask DisposeAsync()
        {
            _inner.Dispose();
            return new ValueTask();
        }
    }
}
