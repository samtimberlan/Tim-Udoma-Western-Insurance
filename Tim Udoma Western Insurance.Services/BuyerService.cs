using Microsoft.Extensions.Logging;
using Tim_Udoma_Western_Insurance.Services.Interfaces;

namespace Tim_Udoma_Western_Insurance.Services
{
    class BuyerService : BaseService, IBuyerService
    {
        private readonly ILogger<BaseService> _logger;

        public BuyerService(ILogger<BaseService> logger) : base(logger)
        {
            _logger = logger;
        }
    }
}
