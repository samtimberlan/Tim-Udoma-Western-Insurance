using Microsoft.AspNetCore.Mvc;
using Tim_Udoma_Western_Insurance.DTOs.Requests;
using Tim_Udoma_Western_Insurance.Services.Interfaces;

namespace Tim_Udoma_Western_Insurance.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController(IProductService productService) : Controller
    {
        private readonly IProductService _productService = productService;

        public async Task<IActionResult> Get(string SKU)
        {
            var result = await _productService.Get(SKU);
            return StatusCode(result.Status, result);
        }

        public async Task<IActionResult> GetAll(string searchTerm, int pageNumber, int pageSize, bool onlyActive = true)
        {
            var result = await _productService.GetAllAsync(searchTerm, pageNumber, pageSize, onlyActive);
            return StatusCode(result.Status, result);
        }

        public async Task<IActionResult> Add(ProductDTO product)
        {
            var result = await _productService.AddAsync(product);
            return StatusCode(result.Status, result);
        }

        public async Task<IActionResult> Deactivate(string SKU)
        {
            var result = await _productService.Deactivate(SKU);
            return StatusCode(result.Status, result);
        }
    }
}
