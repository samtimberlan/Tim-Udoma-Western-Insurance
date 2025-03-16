using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Tim_Udoma_Western_Insurance.Services.Interfaces;

namespace Tim_Udoma_Western_Insurance.Services
{
    class NotificationService(ILogger<BaseService> logger) : BaseService(logger), INotificationService
    {
        private readonly ILogger<BaseService> _logger = logger;

        public void Notify(string userId, string message)
        {
            Debug.WriteLine($"Sending message...{userId} - {message}");
            _logger.LogInformation($"Notified user {userId} of: {message}");
        }
    }
}
