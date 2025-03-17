using Microsoft.AspNetCore.Mvc;
using Tim_Udoma_Western_Insurance.DTOs.Requests;
using Tim_Udoma_Western_Insurance.Services.Interfaces;

namespace Tim_Udoma_Western_Insurance.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController(IProductService productService) : Controller
    {
        private readonly IProductService _productService = productService;

        [HttpGet("{SKU:alpha}")]
        public async Task<IActionResult> Get(string SKU)
        {
            var result = await _productService.Get(SKU);
            return StatusCode(result.Status, result);
        }

        [HttpGet()]
        public async Task<IActionResult> GetAll(string? searchTerm, int pageNumber, int pageSize, bool onlyActive = true)
        {
            var result = await _productService.GetAllAsync(searchTerm, pageNumber, pageSize, onlyActive);
            return StatusCode(result.Status, result);
        }

        [HttpPost()]
        public async Task<IActionResult> Add(Product product)
        {
            var result = await _productService.AddAsync(product);
            return StatusCode(result.Status, result);
        }

        [HttpPut("{SKU:alpha}")]
        public async Task<IActionResult> Deactivate(string SKU)
        {
            var result = await _productService.Deactivate(SKU);
            return StatusCode(result.Status, result);
        }
    }
}
