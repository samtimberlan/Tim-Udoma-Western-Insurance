using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Tim_Udoma_Western_Insurance.Data.Models;
using Tim_Udoma_Western_Insurance.DTOs.Responses.Http;
using Tim_Udoma_Western_Insurance.Services;

namespace Tim_Udoma_Western_Insurance.Tests.Services
{
    public class BuyerServiceTests
    {
        private readonly WIShopDBContext _dbContext;
        private readonly Mock<ILogger<BaseService>> _mockLogger;
        private readonly BuyerService _buyerService;

        public BuyerServiceTests()
        {
            // Create in-memory database
            var options = new DbContextOptionsBuilder<WIShopDBContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new WIShopDBContext(options);

            // Set up mock logger
            _mockLogger = new Mock<ILogger<BaseService>>();

            // Initialize service with actual DbContext and mocked logger
            _buyerService = new BuyerService(
                _dbContext,
                _mockLogger.Object
            );
        }

        [Fact]
        public async Task CreateAsync_WhenValidBuyer_ReturnsSuccessResult()
        {
            // Arrange
            var buyerDto = new DTOs.Requests.Buyer
            {
                Email = "test@example.com",
                Name = "Test Buyer"
            };

            // Act
            var result = await _buyerService.CreateAsync(buyerDto);

            // Assert
            Assert.IsType<SuccessResult>(result);
            Assert.Equal("Buyer created successfully", result.Message);

            // Verify buyer was added to database
            var addedBuyer = await _dbContext.Buyers.FirstOrDefaultAsync(b => b.Email == buyerDto.Email.Trim().ToUpperInvariant());
            Assert.NotNull(addedBuyer);
            Assert.Equal(buyerDto.Name.Trim().ToUpperInvariant(), addedBuyer.Name);
            Assert.Equal(Guid.Empty.GetType(), addedBuyer.Reference.GetType());
        }

        [Fact]
        public async Task CreateAsync_WhenEmailExists_ReturnsConflictError()
        {
            // Arrange
            var existingEmail = "existing@example.com";

            // Add existing buyer directly to context
            _dbContext.Buyers.Add(new Data.Models.Buyer
            {
                Email = existingEmail.ToUpperInvariant(),
                Name = "Existing Buyer",
                DateCreated = DateTime.Now,
                CreatedBy = 1,
                Reference = Guid.NewGuid()
            });
            await _dbContext.SaveChangesAsync();

            var buyerDto = new DTOs.Requests.Buyer
            {
                Email = existingEmail,  // Same email as existing buyer
                Name = "Test Buyer"
            };

            // Act
            var result = await _buyerService.CreateAsync(buyerDto);

            // Assert
            Assert.IsType<ErrorResult>(result);
            var errorResult = (ErrorResult)result;
            Assert.Equal("Buyer with email already exists", errorResult.Message);
            Assert.Equal(StatusCodes.Status409Conflict, errorResult.Status);
        }

        [Fact]
        public async Task CreateAsync_WithWhitespaceInEmailAndName_TrimsValues()
        {
            // Arrange
            var buyerDto = new DTOs.Requests.Buyer
            {
                Email = "  whitespace@example.com  ",
                Name = "  Whitespace Buyer  "
            };

            // Act
            var result = await _buyerService.CreateAsync(buyerDto);

            // Assert
            Assert.IsType<SuccessResult>(result);

            // Verify buyer was added with trimmed values
            var addedBuyer = await _dbContext.Buyers.FirstOrDefaultAsync(b =>
                b.Email == buyerDto.Email.Trim().ToUpperInvariant());

            Assert.NotNull(addedBuyer);
            Assert.Equal("WHITESPACE@EXAMPLE.COM", addedBuyer.Email);
            Assert.Equal("WHITESPACE BUYER", addedBuyer.Name);
        }

        [Fact]
        public async Task CreateAsync_CaseInsensitiveEmailCheck_ReturnsConflictError()
        {
            // Arrange
            var existingEmail = "CASE@EXAMPLE.COM";

            // Add existing buyer directly to context
            _dbContext.Buyers.Add(new Data.Models.Buyer
            {
                Email = existingEmail,
                Name = "Existing Buyer",
                DateCreated = DateTime.Now,
                CreatedBy = 1,
                Reference = Guid.NewGuid()
            });
            await _dbContext.SaveChangesAsync();

            var buyerDto = new DTOs.Requests.Buyer
            {
                Email = "case@example.com",  // Same email but different case
                Name = "Test Buyer"
            };

            // Act
            var result = await _buyerService.CreateAsync(buyerDto);

            // Assert
            Assert.IsType<ErrorResult>(result);
            var errorResult = (ErrorResult)result;
            Assert.Equal("Buyer with email already exists", errorResult.Message);
            Assert.Equal(StatusCodes.Status409Conflict, errorResult.Status);
        }

        [Fact]
        public async Task CreateAsync_SetsCorrectCreatedByAndDateTime()
        {
            // Arrange
            var buyerDto = new DTOs.Requests.Buyer
            {
                Email = "datetime@example.com",
                Name = "DateTime Buyer"
            };

            var beforeCreate = DateTime.Now;

            // Act
            var result = await _buyerService.CreateAsync(buyerDto);

            // Assert
            Assert.IsType<SuccessResult>(result);

            // Verify created by and date time
            var addedBuyer = await _dbContext.Buyers.FirstOrDefaultAsync(b =>
                b.Email == buyerDto.Email.Trim().ToUpperInvariant());

            Assert.NotNull(addedBuyer);
            Assert.Equal(1, addedBuyer.CreatedBy);
            Assert.True(addedBuyer.DateCreated >= beforeCreate);
            Assert.True(addedBuyer.DateCreated <= DateTime.Now);
        }
    }
}