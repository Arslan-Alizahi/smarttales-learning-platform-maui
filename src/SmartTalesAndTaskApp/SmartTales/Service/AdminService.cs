using SmartTales.Data;
using SmartTales.Model;
using SmartTales.Repository.IRepository;
using System.Text.Json;

namespace SmartTales.Service
{
    public class AdminService
    {
        private readonly IAdminRepository _adminRepository;
        private readonly IUserRepository _userRepository;

        public AdminService(IAdminRepository adminRepository, IUserRepository userRepository)
        {
            _adminRepository = adminRepository;
            _userRepository = userRepository;
        }

        #region Admin Authentication

        public async Task<AdminUserModel?> LoginAsync(string username, string password)
        {
            try
            {
                var admin = await _adminRepository.LoginAsync(username, password);
                if (admin != null)
                {
                    await _adminRepository.UpdateLastLoginAsync(admin.Id);
                    await LogAdminActionAsync(admin.Id, "LOGIN", "AdminUser", admin.Id, null, null, true);
                }
                return admin;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during admin login: {ex.Message}");
                return null;
            }
        }

        public async Task LogoutAsync(int adminId)
        {
            await LogAdminActionAsync(adminId, "LOGOUT", "AdminUser", adminId, null, null, true);
        }

        #endregion

        #region User Management

        public async Task<IEnumerable<User>> GetAllUsersAsync(int adminId)
        {
            await LogAdminActionAsync(adminId, "VIEW_USERS", "User", null, null, null, true);
            return await _adminRepository.GetAllUsersAsync();
        }

        public async Task<IEnumerable<User>> SearchUsersAsync(string searchTerm, int adminId)
        {
            await LogAdminActionAsync(adminId, "SEARCH_USERS", "User", null, null, 
                JsonSerializer.Serialize(new { SearchTerm = searchTerm }), true);
            return await _adminRepository.SearchUsersAsync(searchTerm);
        }

        public async Task<User?> GetUserAsync(int userId, int adminId)
        {
            await LogAdminActionAsync(adminId, "VIEW_USER", "User", userId, null, null, true);
            return await _adminRepository.GetUserAsync(userId);
        }

        public async Task<User> CreateUserAsync(User user, int adminId)
        {
            try
            {
                var result = await _adminRepository.CreateUserAsync(user);

                await LogAdminActionAsync(adminId, "CREATE_USER", "User", result.Id, null,
                    JsonSerializer.Serialize(new { FirstName = result.FirstName, LastName = result.LastName, Email = result.Email, Role = result.Role }), true);

                return result;
            }
            catch (Exception ex)
            {
                await LogAdminActionAsync(adminId, "CREATE_USER", "User", null, null, null, false, ex.Message);
                throw;
            }
        }

        public async Task<User?> UpdateUserAsync(User user, int adminId)
        {
            try
            {
                var oldUser = await _adminRepository.GetUserAsync(user.Id);
                var result = await _adminRepository.UpdateUserAsync(user);
                
                if (result != null)
                {
                    await LogAdminActionAsync(adminId, "UPDATE_USER", "User", user.Id, 
                        JsonSerializer.Serialize(oldUser), JsonSerializer.Serialize(result), true);
                }
                
                return result;
            }
            catch (Exception ex)
            {
                await LogAdminActionAsync(adminId, "UPDATE_USER", "User", user.Id, null, null, false, ex.Message);
                throw;
            }
        }

        public async Task<bool> DeleteUserAsync(int userId, int adminId)
        {
            try
            {
                var user = await _adminRepository.GetUserAsync(userId);
                var result = await _adminRepository.DeleteUserAsync(userId);
                
                if (result)
                {
                    await LogAdminActionAsync(adminId, "DELETE_USER", "User", userId, 
                        JsonSerializer.Serialize(user), null, true);
                }
                
                return result;
            }
            catch (Exception ex)
            {
                await LogAdminActionAsync(adminId, "DELETE_USER", "User", userId, null, null, false, ex.Message);
                throw;
            }
        }

        public async Task<bool> ResetUserPasswordAsync(int userId, string newPassword, int adminId)
        {
            try
            {
                var result = await _adminRepository.ResetUserPasswordAsync(userId, newPassword);
                
                await LogAdminActionAsync(adminId, "RESET_PASSWORD", "User", userId, null, 
                    JsonSerializer.Serialize(new { PasswordReset = true }), result);
                
                return result;
            }
            catch (Exception ex)
            {
                await LogAdminActionAsync(adminId, "RESET_PASSWORD", "User", userId, null, null, false, ex.Message);
                throw;
            }
        }

        public async Task<bool> ActivateUserAsync(int userId, int adminId)
        {
            try
            {
                var result = await _adminRepository.ActivateUserAsync(userId);
                await LogAdminActionAsync(adminId, "ACTIVATE_USER", "User", userId, null, null, result);
                return result;
            }
            catch (Exception ex)
            {
                await LogAdminActionAsync(adminId, "ACTIVATE_USER", "User", userId, null, null, false, ex.Message);
                throw;
            }
        }

        public async Task<bool> DeactivateUserAsync(int userId, int adminId)
        {
            try
            {
                var result = await _adminRepository.DeactivateUserAsync(userId);
                await LogAdminActionAsync(adminId, "DEACTIVATE_USER", "User", userId, null, null, result);
                return result;
            }
            catch (Exception ex)
            {
                await LogAdminActionAsync(adminId, "DEACTIVATE_USER", "User", userId, null, null, false, ex.Message);
                throw;
            }
        }

        #endregion

        #region Bulk Operations

        public async Task<bool> BulkDeleteUsersAsync(IEnumerable<int> userIds, int adminId)
        {
            try
            {
                var result = await _adminRepository.BulkDeleteUsersAsync(userIds);
                await LogAdminActionAsync(adminId, "BULK_DELETE", "User", null, null, 
                    JsonSerializer.Serialize(new { UserIds = userIds, Count = userIds.Count() }), result);
                return result;
            }
            catch (Exception ex)
            {
                await LogAdminActionAsync(adminId, "BULK_DELETE", "User", null, null, null, false, ex.Message);
                throw;
            }
        }

        public async Task<bool> BulkUpdateUserRoleAsync(IEnumerable<int> userIds, string newRole, int adminId)
        {
            try
            {
                var result = await _adminRepository.BulkUpdateUserRoleAsync(userIds, newRole);
                await LogAdminActionAsync(adminId, "BULK_UPDATE", "User", null, null, 
                    JsonSerializer.Serialize(new { UserIds = userIds, NewRole = newRole, Count = userIds.Count() }), result);
                return result;
            }
            catch (Exception ex)
            {
                await LogAdminActionAsync(adminId, "BULK_UPDATE", "User", null, null, null, false, ex.Message);
                throw;
            }
        }

        public async Task<IEnumerable<ParentChildRelationshipViewModel>> GetAllParentChildRelationshipsAsync(int adminId)
        {
            try
            {
                await LogAdminActionAsync(adminId, "VIEW_RELATIONSHIPS", "ParentChild", null, null, null, true);

                var relationships = await _adminRepository.GetAllParentChildRelationshipsAsync();
                var result = new List<ParentChildRelationshipViewModel>();

                foreach (var relationship in relationships)
                {
                    var parent = await _adminRepository.GetUserAsync(relationship.ParentId);
                    var child = await _adminRepository.GetUserAsync(relationship.ChildId);

                    result.Add(new ParentChildRelationshipViewModel
                    {
                        Id = relationship.Id,
                        ParentId = relationship.ParentId,
                        ChildId = relationship.ChildId,
                        ParentName = parent != null ? $"{parent.FirstName} {parent.LastName}" : "Unknown Parent",
                        ParentEmail = parent?.Email ?? "No Email",
                        ChildName = child != null ? $"{child.FirstName} {child.LastName}" : "Unknown Child",
                        ChildEmail = child?.Email ?? "No Email",
                        CreatedAt = relationship.CreatedAt,
                        IsActive = relationship.IsActive
                    });
                }

                return result;
            }
            catch (Exception ex)
            {
                await LogAdminActionAsync(adminId, "VIEW_RELATIONSHIPS", "ParentChild", null, null, null, false, ex.Message);
                throw;
            }
        }

        public async Task<bool> CreateParentChildRelationshipByEmailAsync(string parentEmail, string childEmail, int adminId)
        {
            try
            {
                var parent = await _adminRepository.GetUserByEmailAsync(parentEmail);
                var child = await _adminRepository.GetUserByEmailAsync(childEmail);

                if (parent == null || child == null)
                {
                    await LogAdminActionAsync(adminId, "CREATE_RELATIONSHIP", "ParentChild", null, null, null, false, "Parent or child not found");
                    return false;
                }

                if (parent.Role != "Parent" || child.Role != "Kid")
                {
                    await LogAdminActionAsync(adminId, "CREATE_RELATIONSHIP", "ParentChild", null, null, null, false, "Invalid user roles");
                    return false;
                }

                var result = await _adminRepository.CreateParentChildRelationshipAsync(parent.Id, child.Id);
                await LogAdminActionAsync(adminId, "CREATE_RELATIONSHIP", "ParentChild", null, null,
                    JsonSerializer.Serialize(new { ParentEmail = parentEmail, ChildEmail = childEmail }), result);

                return result;
            }
            catch (Exception ex)
            {
                await LogAdminActionAsync(adminId, "CREATE_RELATIONSHIP", "ParentChild", null, null, null, false, ex.Message);
                throw;
            }
        }

        public async Task<bool> DeactivateParentChildRelationshipAsync(int parentId, int childId, int adminId)
        {
            try
            {
                var relationship = await _adminRepository.GetParentChildRelationshipsByParentAsync(parentId);
                var targetRelationship = relationship.FirstOrDefault(r => r.ChildId == childId && r.IsActive);

                if (targetRelationship == null) return false;

                targetRelationship.IsActive = false;
                var result = await _adminRepository.UpdateParentChildRelationshipAsync(targetRelationship);

                await LogAdminActionAsync(adminId, "DEACTIVATE_RELATIONSHIP", "ParentChild", null, null,
                    JsonSerializer.Serialize(new { ParentId = parentId, ChildId = childId }), result);

                return result;
            }
            catch (Exception ex)
            {
                await LogAdminActionAsync(adminId, "DEACTIVATE_RELATIONSHIP", "ParentChild", null, null, null, false, ex.Message);
                throw;
            }
        }

        public async Task<bool> ActivateParentChildRelationshipAsync(int parentId, int childId, int adminId)
        {
            try
            {
                var relationship = await _adminRepository.GetParentChildRelationshipsByParentAsync(parentId);
                var targetRelationship = relationship.FirstOrDefault(r => r.ChildId == childId && !r.IsActive);

                if (targetRelationship == null) return false;

                targetRelationship.IsActive = true;
                var result = await _adminRepository.UpdateParentChildRelationshipAsync(targetRelationship);

                await LogAdminActionAsync(adminId, "ACTIVATE_RELATIONSHIP", "ParentChild", null, null,
                    JsonSerializer.Serialize(new { ParentId = parentId, ChildId = childId }), result);

                return result;
            }
            catch (Exception ex)
            {
                await LogAdminActionAsync(adminId, "ACTIVATE_RELATIONSHIP", "ParentChild", null, null, null, false, ex.Message);
                throw;
            }
        }

        public async Task<bool> RemoveParentChildRelationshipAsync(int parentId, int childId, int adminId)
        {
            try
            {
                var result = await _adminRepository.RemoveParentChildRelationshipAsync(parentId, childId);

                await LogAdminActionAsync(adminId, "REMOVE_RELATIONSHIP", "ParentChild", null, null,
                    JsonSerializer.Serialize(new { ParentId = parentId, ChildId = childId }), result);

                return result;
            }
            catch (Exception ex)
            {
                await LogAdminActionAsync(adminId, "REMOVE_RELATIONSHIP", "ParentChild", null, null, null, false, ex.Message);
                throw;
            }
        }

        #endregion

        #region Admin Management

        public async Task<AdminUserModel> CreateAdminAsync(AdminUserModel admin, int createdBy)
        {
            try
            {
                admin.Password = BCrypt.Net.BCrypt.HashPassword(admin.Password);
                admin.CreatedBy = createdBy;
                admin.UpdatedBy = createdBy;
                
                var result = await _adminRepository.CreateAdminAsync(admin);
                await LogAdminActionAsync(createdBy, "CREATE_ADMIN", "AdminUser", result.Id, null, 
                    JsonSerializer.Serialize(new { Username = result.Username, Email = result.Email, Role = result.Role }), true);
                
                return result;
            }
            catch (Exception ex)
            {
                await LogAdminActionAsync(createdBy, "CREATE_ADMIN", "AdminUser", null, null, null, false, ex.Message);
                throw;
            }
        }

        public async Task<IEnumerable<AdminUserModel>> GetAllAdminsAsync(int adminId)
        {
            await LogAdminActionAsync(adminId, "VIEW_ADMINS", "AdminUser", null, null, null, true);
            return await _adminRepository.GetAllAdminsAsync();
        }

        #endregion

        #region Statistics and Analytics

        public async Task<Dictionary<string, int>> GetUserStatsByRoleAsync(int adminId)
        {
            await LogAdminActionAsync(adminId, "VIEW_STATS", "User", null, null, null, true);
            return await _adminRepository.GetUserStatsByRoleAsync();
        }

        public async Task<Dictionary<string, int>> GetUserRegistrationStatsAsync(int days, int adminId)
        {
            await LogAdminActionAsync(adminId, "VIEW_REGISTRATION_STATS", "User", null, null, 
                JsonSerializer.Serialize(new { Days = days }), true);
            return await _adminRepository.GetUserRegistrationStatsAsync(days);
        }

        public async Task<IEnumerable<User>> GetRecentlyRegisteredUsersAsync(int count, int adminId)
        {
            await LogAdminActionAsync(adminId, "VIEW_RECENT_USERS", "User", null, null, 
                JsonSerializer.Serialize(new { Count = count }), true);
            return await _adminRepository.GetRecentlyRegisteredUsersAsync(count);
        }

        #endregion

        #region Parent-Child Relationship Management

        public async Task<bool> CreateParentChildRelationshipAsync(int parentId, int childId, int adminId)
        {
            try
            {
                var result = await _adminRepository.CreateParentChildRelationshipAsync(parentId, childId);
                await LogAdminActionAsync(adminId, "CREATE_RELATIONSHIP", "ParentChild", null, null, 
                    JsonSerializer.Serialize(new { ParentId = parentId, ChildId = childId }), result);
                return result;
            }
            catch (Exception ex)
            {
                await LogAdminActionAsync(adminId, "CREATE_RELATIONSHIP", "ParentChild", null, null, null, false, ex.Message);
                throw;
            }
        }



        #endregion

        #region Audit Logging

        public async Task<AdminAuditLogModel> LogAdminActionAsync(int adminId, string action, string entityType, 
            int? entityId, string? oldValues, string? newValues, bool success, string? errorMessage = null)
        {
            var auditLog = new AdminAuditLogModel
            {
                AdminUserId = adminId,
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                OldValues = oldValues ?? string.Empty,
                NewValues = newValues ?? string.Empty,
                Success = success,
                ErrorMessage = errorMessage ?? string.Empty,
                IPAddress = "127.0.0.1", // Placeholder - in real app, get from HttpContext
                UserAgent = "SmartTales Admin", // Placeholder
                Timestamp = DateTime.Now
            };

            return await _adminRepository.CreateAuditLogAsync(auditLog);
        }

        public async Task<IEnumerable<AdminAuditLogModel>> GetAuditLogsAsync(int page, int pageSize, int adminId)
        {
            await LogAdminActionAsync(adminId, "VIEW_AUDIT_LOGS", "AdminAuditLog", null, null, 
                JsonSerializer.Serialize(new { Page = page, PageSize = pageSize }), true);
            return await _adminRepository.GetAuditLogsAsync(page, pageSize);
        }

        public async Task<IEnumerable<AdminAuditLogModel>> GetRecentAdminActionsAsync(int count, int adminId)
        {
            return await _adminRepository.GetRecentAdminActionsAsync(count);
        }

        #endregion

        #region Data Export

        public async Task<string> ExportUsersDataAsync(string format, int adminId)
        {
            try
            {
                var users = await _adminRepository.GetAllUsersAsync();
                string exportData = string.Empty;

                if (format.ToLower() == "json")
                {
                    exportData = JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true });
                }
                else if (format.ToLower() == "csv")
                {
                    // Simple CSV export
                    var csv = "Id,FirstName,LastName,Email,Role,PhoneNumber\n";
                    foreach (var user in users)
                    {
                        csv += $"{user.Id},{user.FirstName},{user.LastName},{user.Email},{user.Role},{user.PhoneNumber}\n";
                    }
                    exportData = csv;
                }

                await LogAdminActionAsync(adminId, "EXPORT_DATA", "User", null, null, 
                    JsonSerializer.Serialize(new { Format = format, Count = users.Count() }), true);

                return exportData;
            }
            catch (Exception ex)
            {
                await LogAdminActionAsync(adminId, "EXPORT_DATA", "User", null, null, null, false, ex.Message);
                throw;
            }
        }

        #endregion
    }
}
