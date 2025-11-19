using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SmartTales.Data;
using SmartTales.Model;
using SmartTales.Repository.IRepository;

namespace SmartTales.Repository
{
    public class PasswordResetRepository : IPasswordResetRepository
    {
        private readonly LocalDbService _dbService;

        public PasswordResetRepository(LocalDbService dbService)
        {
            _dbService = dbService;
        }

        public async Task<PasswordResetRequestModel> CreateAsync(PasswordResetRequestModel request)
        {
            try
            {
                var connection = _dbService.GetConnection();
                await connection.InsertAsync(request);
                return request;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating password reset request: {ex.Message}");
                throw;
            }
        }

        public async Task<PasswordResetRequestModel> GetAsync(int requestId)
        {
            try
            {
                var connection = _dbService.GetConnection();
                return await connection.Table<PasswordResetRequestModel>()
                    .Where(r => r.RequestId == requestId)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting password reset request: {ex.Message}");
                throw;
            }
        }

        public async Task<PasswordResetRequestModel> UpdateAsync(PasswordResetRequestModel request)
        {
            try
            {
                var connection = _dbService.GetConnection();
                await connection.UpdateAsync(request);
                return request;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating password reset request: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int requestId)
        {
            try
            {
                var connection = _dbService.GetConnection();
                var rowsAffected = await connection.DeleteAsync<PasswordResetRequestModel>(requestId);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting password reset request: {ex.Message}");
                return false;
            }
        }

        public async Task<List<PasswordResetRequestModel>> GetAllRequestsAsync()
        {
            try
            {
                var connection = _dbService.GetConnection();
                var requests = await connection.Table<PasswordResetRequestModel>()
                    .OrderByDescending(r => r.RequestDateTime)
                    .ToListAsync();
                return requests;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting all password reset requests: {ex.Message}");
                return new List<PasswordResetRequestModel>();
            }
        }

        public async Task<List<PasswordResetRequestModel>> GetPendingRequestsAsync()
        {
            try
            {
                var connection = _dbService.GetConnection();
                var requests = await connection.Table<PasswordResetRequestModel>()
                    .Where(r => r.Status == "Pending")
                    .OrderByDescending(r => r.RequestDateTime)
                    .ToListAsync();
                return requests;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting pending password reset requests: {ex.Message}");
                return new List<PasswordResetRequestModel>();
            }
        }

        public async Task<PasswordResetRequestModel?> GetPendingRequestByUserIdAsync(int userId)
        {
            try
            {
                var connection = _dbService.GetConnection();
                return await connection.Table<PasswordResetRequestModel>()
                    .Where(r => r.UserId == userId && r.Status == "Pending")
                    .OrderByDescending(r => r.RequestDateTime)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting pending request by user ID: {ex.Message}");
                return null;
            }
        }

        public async Task<List<PasswordResetRequestModel>> GetRequestsByUserIdAsync(int userId)
        {
            try
            {
                var connection = _dbService.GetConnection();
                var requests = await connection.Table<PasswordResetRequestModel>()
                    .Where(r => r.UserId == userId)
                    .OrderByDescending(r => r.RequestDateTime)
                    .ToListAsync();
                return requests;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting requests by user ID: {ex.Message}");
                return new List<PasswordResetRequestModel>();
            }
        }

        public async Task<List<PasswordResetRequestModel>> GetRequestsByStatusAsync(string status)
        {
            try
            {
                var connection = _dbService.GetConnection();
                var requests = await connection.Table<PasswordResetRequestModel>()
                    .Where(r => r.Status == status)
                    .OrderByDescending(r => r.RequestDateTime)
                    .ToListAsync();
                return requests;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting requests by status: {ex.Message}");
                return new List<PasswordResetRequestModel>();
            }
        }

        public async Task<List<PasswordResetRequestModel>> GetRecentRequestsAsync(int count)
        {
            try
            {
                var connection = _dbService.GetConnection();
                var requests = await connection.Table<PasswordResetRequestModel>()
                    .OrderByDescending(r => r.RequestDateTime)
                    .Take(count)
                    .ToListAsync();
                return requests;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting recent requests: {ex.Message}");
                return new List<PasswordResetRequestModel>();
            }
        }
    }
}
