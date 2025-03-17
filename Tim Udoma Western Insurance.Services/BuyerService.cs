using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tim_Udoma_Western_Insurance.Data.Models;
using Tim_Udoma_Western_Insurance.DTOs.Responses.Http;
using Tim_Udoma_Western_Insurance.Services.Interfaces;

namespace Tim_Udoma_Western_Insurance.Services
{
    public class BuyerService(WIShopDBContext dBContext, ILogger<BaseService> logger) : BaseService(logger), IBuyerService
    {
        private readonly WIShopDBContext _dBContext = dBContext;
        private readonly ILogger<BaseService> _logger = logger;

        #region API Methods
        public async Task<Result> CreateAsync(DTOs.Requests.Buyer buyer)
        {
            _logger.LogInformation("Creating buyer with email: {email}", buyer.Email);
            bool emailExists = await _dBContext.Buyers.AnyAsync(b => b.Email == buyer.Email);
            if (emailExists)
            {
                _logger.LogError("Buyer with email: {email} already exists", buyer.Email);
                return new ErrorResult("Buyer with email already exists", StatusCodes.Status409Conflict);
            }
            Data.Models.Buyer newBuyer = new()
            {
                Email = buyer.Email.Trim().ToUpperInvariant(),
                Name = buyer.Name.Trim().ToUpperInvariant(),
                DateCreated = DateTime.Now,
                CreatedBy = 1,
                Reference = Guid.NewGuid()
            };
            _dBContext.Buyers.Add(newBuyer);
            await _dBContext.SaveChangesAsync();
            _logger.LogInformation("Buyer with email: {email} created successfully", buyer.Email);
            return new SuccessResult("Buyer created successfully");
        }
        #endregion

        #region Private Methods

        #endregion


    }
}
