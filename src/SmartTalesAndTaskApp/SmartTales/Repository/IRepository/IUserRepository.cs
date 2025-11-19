using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTales.Data;

namespace SmartTales.Repository.IRepository
{
    public interface IUserRepository
    {
        Task<User> CreateAsync(User user);
        Task<bool> DeleteAsync(int id);
        Task<User?> GetAsync(int id);
        Task<IEnumerable<User>> GetAllAsync();
        Task<User?> UpdateAsync(User user);
        Task<IEnumerable<User>> GetUsersByRoleAsync(string role);
        Task<User?> GetUserByEmailAsync(string email);
        Task<User?> LoginAsync(string email, string password);
        Task<User?> GetKidDashboardDetailsAsync(int kidId);
        Task<bool> UpdateKidDashboardMetricsAsync(int kidId, int? progressPercentage = null,
            int? completedAssignments = null, int? pendingAssignments = null, int? totalPoints = null);
        Task<IEnumerable<User>> GetChildrenByParentIdAsync(int parentId);
    }
}
