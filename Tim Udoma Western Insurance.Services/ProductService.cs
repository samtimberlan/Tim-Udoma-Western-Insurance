using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tim_Udoma_Western_Insurance.Data.Models;
using Tim_Udoma_Western_Insurance.DTOs.Extensions;
using Tim_Udoma_Western_Insurance.DTOs.Requests;
using Tim_Udoma_Western_Insurance.DTOs.Responses;
using Tim_Udoma_Western_Insurance.DTOs.Responses.Http;
using Tim_Udoma_Western_Insurance.Services.Interfaces;

namespace Tim_Udoma_Western_Insurance.Services
{
    /// <summary>
    /// Constructor for ProductService.
    /// </summary>
    /// <param name="dBContext"></param>
    /// <param name="logger"></param>
    public class ProductService(WIShopDBContext dBContext, INotificationService notificationService, ILogger<BaseService> logger) : BaseService(logger), IProductService
    {
        private readonly WIShopDBContext _dBContext = dBContext;
        private readonly INotificationService _notificationService = notificationService;
        private readonly ILogger<BaseService> _logger = logger;

        #region API Methods
        public async Task<Result> AddAsync(ProductDTO product)
        {
            _logger.LogInformation("Adding product with SKU: {sku}", product.Sku);
            bool skuExists = await _dBContext.Products.AnyAsync(p => p.Sku == product.Sku);
            if (skuExists)
            {
                _logger.LogError("Product with SKU: {sku} already exists", product.Sku);
                return new ErrorResult("Product with SKU already exists", StatusCodes.Status409Conflict);
            }
            Product newProduct = new()
            {
                Sku = product.Sku.Trim().ToUpperInvariant(),
                Title = product.Title.Trim().ToUpperInvariant(),
                Description = product.Description.Trim().ToUpperInvariant(),
                BuyerId = 1,
                Active = true,
                DateCreated = DateTime.Now,
                CreatedBy = 1,
                Reference = Guid.NewGuid()
            };
            _dBContext.Products.Add(newProduct);
            await _dBContext.SaveChangesAsync();

            _logger.LogInformation("Product with SKU: {sku} added successfully", product.Sku);
            return new SuccessResult("Product added successfully");
        }

        public async Task<Result> GetAllAsync(string searchTerm, int pageNumber, int pageSize, bool onlyActive = true)
        {
            _logger.LogInformation("Getting all products");
            IQueryable<ProductResponse> productQuery = _dBContext.Products
                .Where(p => p.Title.Contains(searchTerm) || p.Sku.Contains(searchTerm) && p.Active == onlyActive)
                .AsNoTrackingWithIdentityResolution()
                .OrderBy(p => p.Title)
                .Select(p => new ProductResponse
                {
                    SKU = p.Sku,
                    Title = p.Title,
                    Description = p.Description,
                    Active = p.Active,
                    DateCreated = p.DateCreated,
                    Reference = p.Reference.ToString()
                });

            var products = await productQuery.ToPaginatedListAsync(pageNumber, pageSize);
            _logger.LogInformation("Retrieved {count} products", products.Count);
            return new SuccessResult(products);
        }
        public async Task<Result> Get(string SKU, bool onlyActive = true)
        {
            _logger.LogInformation("Getting product with SKU: {SKU}", SKU);
            Product? product = await _dBContext.Products.FirstOrDefaultAsync(p => p.Sku == SKU && p.Active == onlyActive);
            if (product == null)
            {
                _logger.LogError("Product with SKU: {SKU} not found", SKU);
                return new ErrorResult("Product not found", StatusCodes.Status404NotFound);
            }
            else
            {
                ProductResponse productResponse = new()
                {
                    SKU = product.Sku,
                    Title = product.Title,
                    Description = product.Description,
                    Active = product.Active,
                    DateCreated = product.DateCreated,
                    Reference = product.Reference.ToString()
                };
                _logger.LogInformation("Retrieved product with SKU: {SKU}", SKU);
                return new SuccessResult(productResponse);
            }
        }

        public async Task<Result> DeleteAsync(string SKU)
        {
            _logger.LogInformation("Deleting product with SKU: {SKU}", SKU);
            Product? product = await _dBContext.Products.FirstOrDefaultAsync(p => p.Sku == SKU);
            if (product == null)
            {
                _logger.LogError("Product with SKU: {SKU} not found", SKU);
                return new ErrorResult("Product not found", StatusCodes.Status404NotFound);
            }
            else
            {
                // In production, this should be a soft delete
                _dBContext.Products.Remove(product);
                await _dBContext.SaveChangesAsync();
                _logger.LogInformation("Product with SKU: {SKU} deleted successfully", SKU);
                return new SuccessResult("Product deleted successfully");
            }
        }

        public async Task<Result> Deactivate(string SKU)
        {
            _logger.LogInformation("Deactivating product with SKU: {SKU}", SKU);
            Product? product = await _dBContext.Products.FirstOrDefaultAsync(p => p.Sku == SKU);
            if (product == null)
            {
                _logger.LogError("Product with SKU: {SKU} not found", SKU);
                return new ErrorResult("Product not found", StatusCodes.Status404NotFound);
            }
            else
            {
                product.Active = false;
                await _dBContext.SaveChangesAsync();
                NotifyUsers("1", "2");
                _logger.LogInformation("Product with SKU: {SKU} deactivated successfully", SKU);
                return new SuccessResult("Product deactivated successfully");
            }
        }
        #endregion

        #region Private Methods
        private void NotifyUsers(string buyerId, string sellerId)
        {
            string message = "Product Deactivated";
            _notificationService.Notify(buyerId, message);
            _notificationService.Notify(sellerId, message);
        }
        #endregion

    }
}
