using Microsoft.AspNetCore.Mvc;
using Tim_Udoma_Western_Insurance.DTOs.Requests;
using Tim_Udoma_Western_Insurance.Services.Interfaces;

namespace Tim_Udoma_Western_Insurance.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BuyerController(IBuyerService buyerService) : ControllerBase
    {
        private readonly IBuyerService _buyerService = buyerService;

        [HttpPost]
        public async Task<IActionResult> Create(Buyer buyer)
        {
            var result = await _buyerService.CreateAsync(buyer);
            return StatusCode(result.Status, result);
        }
    }
}
