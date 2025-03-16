using Microsoft.Extensions.Logging;
using Tim_Udoma_Western_Insurance.Services.Interfaces;

namespace Tim_Udoma_Western_Insurance.Services
{
    class NotificationService : BaseService, INotificationService, INotify
    {
        private readonly ILogger<BaseService> _logger;

        public NotificationService(ILogger<BaseService> logger) : base(logger)
        {
            _logger = logger;
        }
        public void Notify(string userId, string message)
        {
            throw new NotImplementedException();
        }
    }
}
