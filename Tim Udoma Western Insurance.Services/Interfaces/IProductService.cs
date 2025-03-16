using Tim_Udoma_Western_Insurance.DTOs.Requests;
using Tim_Udoma_Western_Insurance.DTOs.Responses.Http;

namespace Tim_Udoma_Western_Insurance.Services.Interfaces
{
    public interface IProductService
    {
        /// <summary>
        /// Add a new product with the specified details.
        /// </summary>
        /// <param name="product"></param>
        /// <returns></returns>
        Task<Result> AddAsync(ProductDTO product);
        /// <summary>
        /// Delete a product with the specified SKU.
        /// </summary>
        /// <param name="SKU"></param>
        /// <returns></returns>
        Task<Result> DeleteAsync(string SKU);
        /// <summary>
        /// Get all products with the specified search term. Results are paginated and only active products are returned by default.
        /// </summary>
        /// <param name="searchTerm"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <param name="onlyActive"></param>
        /// <returns></returns>
        Task<Result> GetAllAsync(string searchTerm, int pageNumber, int pageSize, bool onlyActive = true);
        /// <summary>
        /// Get a product with the specified SKU. Only an active product is returned by default.
        /// </summary>
        /// <param name="SKU"></param>
        /// <returns></returns>
        Task<Result> Get(string SKU, bool onlyActive = true);
        /// <summary>
        /// Deactivate a product with the specified SKU.
        /// </summary>
        /// <param name="SKU"></param>
        /// <returns></returns>
        Task<Result> Deactivate(string SKU);
    }
}
