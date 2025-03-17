using Tim_Udoma_Western_Insurance.DTOs.Requests;
using Tim_Udoma_Western_Insurance.DTOs.Responses.Http;

namespace Tim_Udoma_Western_Insurance.Services.Interfaces
{
    public interface IBuyerService
    {
        /// <summary>
        /// Create a new buyer.
        /// </summary>
        /// <param name="buyer"></param>
        /// <returns></returns>
        Task<Result> CreateAsync(Buyer buyer);
    }
}
