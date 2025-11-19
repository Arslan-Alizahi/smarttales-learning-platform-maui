using System.Collections.Generic;
using System.Threading.Tasks;
using SmartTales.Model;

namespace SmartTales.Repository.IRepository
{
    public interface IPasswordResetRepository
    {
        Task<PasswordResetRequestModel> CreateAsync(PasswordResetRequestModel request);
        Task<PasswordResetRequestModel> GetAsync(int requestId);
        Task<PasswordResetRequestModel> UpdateAsync(PasswordResetRequestModel request);
        Task<bool> DeleteAsync(int requestId);
        Task<List<PasswordResetRequestModel>> GetAllRequestsAsync();
        Task<List<PasswordResetRequestModel>> GetPendingRequestsAsync();
        Task<PasswordResetRequestModel?> GetPendingRequestByUserIdAsync(int userId);
        Task<List<PasswordResetRequestModel>> GetRequestsByUserIdAsync(int userId);
        Task<List<PasswordResetRequestModel>> GetRequestsByStatusAsync(string status);
        Task<List<PasswordResetRequestModel>> GetRecentRequestsAsync(int count);
    }
}
