using Microsoft.Extensions.Logging;

namespace Tim_Udoma_Western_Insurance.Services
{
    /// <summary>
    /// Base service class. General service methods can be added here, e.g GetCurrentUser
    /// </summary>
    public class BaseService
    {
        /// <summary>
        /// Logger instance.
        /// </summary>
        private readonly ILogger<BaseService> _logger;

        /// <summary>
        /// Constructor for BaseService.
        /// </summary>
        /// <param name="logger"></param>
        public BaseService(ILogger<BaseService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Log an error message.
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="message"></param>
        public void LogError(Exception ex, string message)
        {
            _logger.LogError(ex, message);
        }
    }
}
