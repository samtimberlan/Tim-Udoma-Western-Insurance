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
    public class ProductService : BaseService, IProductService
    {
        private readonly WIShopDBContext _dBContext;
        private readonly ILogger<BaseService> _logger;

        public ProductService(WIShopDBContext dBContext, ILogger<BaseService> logger) : base(logger)
        {
            _dBContext = dBContext;
            _logger = logger;
        }

        public async Task<Result> AddProductAsync(ProductDTO product)
        {
            _logger.LogInformation("Adding product with SKU: {sku}", product.Sku);
            bool nameExists = await _dBContext.Products.AnyAsync(p => p.Sku == product.Sku);
            if (nameExists)
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

        public async Task<Result> GetAllProductsAsync(string searchTerm, int pageNumber, int pageSize, bool onlyActive = true)
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

        public async Task<Result> DeleteProduct()
        {
            throw new NotImplementedException();
        }
    }
}
