using SmartTales.Data;
using SmartTales.Model;
using SmartTales.Repository.IRepository;
using SQLite;

namespace SmartTales.Repository
{
    public class AdminRepository : IAdminRepository
    {
        private readonly LocalDbService _localDbService;

        public AdminRepository(LocalDbService localDbService)
        {
            _localDbService = localDbService ?? throw new ArgumentNullException(nameof(localDbService));
        }

        #region Admin User Management

        public async Task<AdminUserModel> CreateAdminAsync(AdminUserModel admin)
        {
            if (admin == null) throw new ArgumentNullException(nameof(admin));
            
            admin.CreatedAt = DateTime.Now;
            admin.UpdatedAt = DateTime.Now;
            await _localDbService.GetConnection().InsertAsync(admin);
            return admin;
        }

        public async Task<bool> DeleteAdminAsync(int id)
        {
            var admin = await _localDbService.GetConnection().FindAsync<AdminUserModel>(id);
            if (admin == null) return false;

            await _localDbService.GetConnection().DeleteAsync(admin);
            return true;
        }

        public async Task<AdminUserModel?> GetAdminAsync(int id)
        {
            return await _localDbService.GetConnection().FindAsync<AdminUserModel>(id);
        }

        public async Task<IEnumerable<AdminUserModel>> GetAllAdminsAsync()
        {
            return await _localDbService.GetConnection().Table<AdminUserModel>().ToListAsync();
        }

        public async Task<AdminUserModel?> UpdateAdminAsync(AdminUserModel admin)
        {
            if (admin == null) throw new ArgumentNullException(nameof(admin));

            var adminFromDb = await _localDbService.GetConnection().FindAsync<AdminUserModel>(admin.Id);
            if (adminFromDb == null) return null;

            adminFromDb.Username = admin.Username;
            adminFromDb.Email = admin.Email;
            adminFromDb.FirstName = admin.FirstName;
            adminFromDb.LastName = admin.LastName;
            adminFromDb.Role = admin.Role;
            adminFromDb.IsActive = admin.IsActive;
            adminFromDb.UpdatedAt = DateTime.Now;
            adminFromDb.UpdatedBy = admin.UpdatedBy;

            // Only update password if it's provided
            if (!string.IsNullOrEmpty(admin.Password))
            {
                adminFromDb.Password = admin.Password;
            }

            await _localDbService.GetConnection().UpdateAsync(adminFromDb);
            return adminFromDb;
        }

        public async Task<AdminUserModel?> GetAdminByUsernameAsync(string username)
        {
            return await _localDbService.GetConnection()
                .Table<AdminUserModel>()
                .Where(a => a.Username == username)
                .FirstOrDefaultAsync();
        }

        public async Task<AdminUserModel?> GetAdminByEmailAsync(string email)
        {
            return await _localDbService.GetConnection()
                .Table<AdminUserModel>()
                .Where(a => a.Email == email)
                .FirstOrDefaultAsync();
        }

        public async Task<AdminUserModel?> LoginAsync(string username, string password)
        {
            var admin = await _localDbService.GetConnection()
                .Table<AdminUserModel>()
                .Where(a => (a.Username == username || a.Email == username) && a.IsActive)
                .FirstOrDefaultAsync();

            if (admin != null && BCrypt.Net.BCrypt.Verify(password, admin.Password))
            {
                return admin;
            }

            return null;
        }

        public async Task<bool> UpdateLastLoginAsync(int adminId)
        {
            var admin = await _localDbService.GetConnection().FindAsync<AdminUserModel>(adminId);
            if (admin == null) return false;

            admin.LastLoginAt = DateTime.Now;
            await _localDbService.GetConnection().UpdateAsync(admin);
            return true;
        }

        public async Task<bool> ActivateAdminAsync(int adminId, int updatedBy)
        {
            var admin = await _localDbService.GetConnection().FindAsync<AdminUserModel>(adminId);
            if (admin == null) return false;

            admin.IsActive = true;
            admin.UpdatedAt = DateTime.Now;
            admin.UpdatedBy = updatedBy;
            await _localDbService.GetConnection().UpdateAsync(admin);
            return true;
        }

        public async Task<bool> DeactivateAdminAsync(int adminId, int updatedBy)
        {
            var admin = await _localDbService.GetConnection().FindAsync<AdminUserModel>(adminId);
            if (admin == null) return false;

            admin.IsActive = false;
            admin.UpdatedAt = DateTime.Now;
            admin.UpdatedBy = updatedBy;
            await _localDbService.GetConnection().UpdateAsync(admin);
            return true;
        }

        public async Task<bool> ResetPasswordAsync(int adminId, string newPassword, int updatedBy)
        {
            var admin = await _localDbService.GetConnection().FindAsync<AdminUserModel>(adminId);
            if (admin == null) return false;

            admin.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
            admin.UpdatedAt = DateTime.Now;
            admin.UpdatedBy = updatedBy;
            await _localDbService.GetConnection().UpdateAsync(admin);
            return true;
        }

        #endregion

        #region User Account Management

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _localDbService.GetConnection().Table<User>().ToListAsync();
        }

        public async Task<IEnumerable<User>> GetUsersByRoleAsync(string role)
        {
            return await _localDbService.GetConnection()
                .Table<User>()
                .Where(u => u.Role == role)
                .ToListAsync();
        }

        public async Task<IEnumerable<User>> SearchUsersAsync(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return await GetAllUsersAsync();

            var lowerSearchTerm = searchTerm.ToLower();
            return await _localDbService.GetConnection()
                .Table<User>()
                .Where(u => u.FirstName.ToLower().Contains(lowerSearchTerm) ||
                           u.LastName.ToLower().Contains(lowerSearchTerm) ||
                           u.Email.ToLower().Contains(lowerSearchTerm))
                .ToListAsync();
        }

        public async Task<User?> GetUserAsync(int id)
        {
            return await _localDbService.GetConnection().FindAsync<User>(id);
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _localDbService.GetConnection()
                .Table<User>()
                .Where(u => u.Email == email)
                .FirstOrDefaultAsync();
        }

        public async Task<User> CreateUserAsync(User user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            // Hash password if provided
            if (!string.IsNullOrEmpty(user.Password))
            {
                user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
            }

            await _localDbService.GetConnection().InsertAsync(user);
            return user;
        }

        public async Task<User?> UpdateUserAsync(User user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            var userFromDb = await _localDbService.GetConnection().FindAsync<User>(user.Id);
            if (userFromDb == null) return null;

            // Update all fields
            userFromDb.FirstName = user.FirstName;
            userFromDb.LastName = user.LastName;
            userFromDb.Email = user.Email;
            userFromDb.PhoneNumber = user.PhoneNumber;
            userFromDb.Role = user.Role;
            userFromDb.GradeLevel = user.GradeLevel;
            userFromDb.Address = user.Address;
            userFromDb.Specialization = user.Specialization;
            userFromDb.YearsOfExperience = user.YearsOfExperience;

            // Only update password if provided
            if (!string.IsNullOrEmpty(user.Password))
            {
                userFromDb.Password = user.Password;
            }

            await _localDbService.GetConnection().UpdateAsync(userFromDb);
            return userFromDb;
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _localDbService.GetConnection().FindAsync<User>(id);
            if (user == null) return false;

            await _localDbService.GetConnection().DeleteAsync(user);
            return true;
        }

        public async Task<bool> ActivateUserAsync(int userId)
        {
            // For now, we'll use a custom field or implement soft delete later
            // This is a placeholder implementation
            return true;
        }

        public async Task<bool> DeactivateUserAsync(int userId)
        {
            // For now, we'll use a custom field or implement soft delete later
            // This is a placeholder implementation
            return true;
        }

        public async Task<bool> ResetUserPasswordAsync(int userId, string newPassword)
        {
            var user = await _localDbService.GetConnection().FindAsync<User>(userId);
            if (user == null) return false;

            user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
            await _localDbService.GetConnection().UpdateAsync(user);
            return true;
        }

        public async Task<IEnumerable<User>> GetUsersWithPaginationAsync(int page, int pageSize)
        {
            var offset = (page - 1) * pageSize;
            return await _localDbService.GetConnection()
                .Table<User>()
                .Skip(offset)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetTotalUsersCountAsync()
        {
            return await _localDbService.GetConnection().Table<User>().CountAsync();
        }

        public async Task<int> GetUsersCountByRoleAsync(string role)
        {
            return await _localDbService.GetConnection()
                .Table<User>()
                .Where(u => u.Role == role)
                .CountAsync();
        }

        #endregion

        #region Bulk Operations

        public async Task<bool> BulkDeleteUsersAsync(IEnumerable<int> userIds)
        {
            try
            {
                foreach (var userId in userIds)
                {
                    await DeleteUserAsync(userId);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> BulkUpdateUserRoleAsync(IEnumerable<int> userIds, string newRole)
        {
            try
            {
                foreach (var userId in userIds)
                {
                    var user = await _localDbService.GetConnection().FindAsync<User>(userId);
                    if (user != null)
                    {
                        user.Role = newRole;
                        await _localDbService.GetConnection().UpdateAsync(user);
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> BulkActivateUsersAsync(IEnumerable<int> userIds)
        {
            // Placeholder implementation
            return true;
        }

        public async Task<bool> BulkDeactivateUsersAsync(IEnumerable<int> userIds)
        {
            // Placeholder implementation
            return true;
        }

        #endregion

        #region Audit Logging

        public async Task<AdminAuditLogModel> CreateAuditLogAsync(AdminAuditLogModel auditLog)
        {
            if (auditLog == null) throw new ArgumentNullException(nameof(auditLog));
            
            auditLog.Timestamp = DateTime.Now;
            await _localDbService.GetConnection().InsertAsync(auditLog);
            return auditLog;
        }

        public async Task<IEnumerable<AdminAuditLogModel>> GetAuditLogsAsync(int page, int pageSize)
        {
            var offset = (page - 1) * pageSize;
            return await _localDbService.GetConnection()
                .Table<AdminAuditLogModel>()
                .OrderByDescending(a => a.Timestamp)
                .Skip(offset)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<AdminAuditLogModel>> GetAuditLogsByAdminAsync(int adminId, int page, int pageSize)
        {
            var offset = (page - 1) * pageSize;
            return await _localDbService.GetConnection()
                .Table<AdminAuditLogModel>()
                .Where(a => a.AdminUserId == adminId)
                .OrderByDescending(a => a.Timestamp)
                .Skip(offset)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<AdminAuditLogModel>> GetAuditLogsByEntityAsync(string entityType, int entityId)
        {
            return await _localDbService.GetConnection()
                .Table<AdminAuditLogModel>()
                .Where(a => a.EntityType == entityType && a.EntityId == entityId)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }

        public async Task<IEnumerable<AdminAuditLogModel>> GetAuditLogsByActionAsync(string action)
        {
            return await _localDbService.GetConnection()
                .Table<AdminAuditLogModel>()
                .Where(a => a.Action == action)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }

        public async Task<IEnumerable<AdminAuditLogModel>> GetAuditLogsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _localDbService.GetConnection()
                .Table<AdminAuditLogModel>()
                .Where(a => a.Timestamp >= startDate && a.Timestamp <= endDate)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }

        public async Task<int> GetTotalAuditLogsCountAsync()
        {
            return await _localDbService.GetConnection().Table<AdminAuditLogModel>().CountAsync();
        }

        #endregion

        #region Statistics and Analytics

        public async Task<Dictionary<string, int>> GetUserStatsByRoleAsync()
        {
            var users = await GetAllUsersAsync();
            return users.GroupBy(u => u.Role)
                       .ToDictionary(g => g.Key, g => g.Count());
        }

        public async Task<Dictionary<string, int>> GetUserRegistrationStatsAsync(int days = 30)
        {
            // Since we don't have registration date in User model, we'll return a placeholder
            var stats = new Dictionary<string, int>();
            var roles = new[] { "Kid", "Parent", "Teacher" };

            foreach (var role in roles)
            {
                var count = await GetUsersCountByRoleAsync(role);
                stats[role] = count;
            }

            return stats;
        }

        public async Task<Dictionary<string, int>> GetAdminActivityStatsAsync(int days = 30)
        {
            var startDate = DateTime.Now.AddDays(-days);
            var logs = await GetAuditLogsByDateRangeAsync(startDate, DateTime.Now);

            return logs.GroupBy(l => l.Action)
                      .ToDictionary(g => g.Key, g => g.Count());
        }

        public async Task<IEnumerable<User>> GetRecentlyRegisteredUsersAsync(int count = 10)
        {
            // Since we don't have registration date, we'll return recent users by ID
            return await _localDbService.GetConnection()
                .Table<User>()
                .OrderByDescending(u => u.Id)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<AdminAuditLogModel>> GetRecentAdminActionsAsync(int count = 10)
        {
            return await _localDbService.GetConnection()
                .Table<AdminAuditLogModel>()
                .OrderByDescending(a => a.Timestamp)
                .Take(count)
                .ToListAsync();
        }

        #endregion

        #region Parent-Child Relationship Management

        public async Task<IEnumerable<ParentChildModel>> GetAllParentChildRelationshipsAsync()
        {
            return await _localDbService.GetConnection().Table<ParentChildModel>().ToListAsync();
        }

        public async Task<IEnumerable<ParentChildModel>> GetParentChildRelationshipsByParentAsync(int parentId)
        {
            return await _localDbService.GetConnection()
                .Table<ParentChildModel>()
                .Where(pc => pc.ParentId == parentId && pc.IsActive)
                .ToListAsync();
        }

        public async Task<IEnumerable<ParentChildModel>> GetParentChildRelationshipsByChildAsync(int childId)
        {
            return await _localDbService.GetConnection()
                .Table<ParentChildModel>()
                .Where(pc => pc.ChildId == childId)
                .ToListAsync();
        }

        public async Task<bool> CreateParentChildRelationshipAsync(int parentId, int childId)
        {
            try
            {
                // Check if relationship already exists
                var existing = await _localDbService.GetConnection()
                    .Table<ParentChildModel>()
                    .Where(pc => pc.ParentId == parentId && pc.ChildId == childId)
                    .FirstOrDefaultAsync();

                if (existing != null)
                {
                    // Reactivate if exists but inactive
                    if (!existing.IsActive)
                    {
                        existing.IsActive = true;
                        await _localDbService.GetConnection().UpdateAsync(existing);
                        return true;
                    }
                    return false; // Already exists and active
                }

                var relationship = new ParentChildModel
                {
                    ParentId = parentId,
                    ChildId = childId,
                    CreatedAt = DateTime.Now,
                    IsActive = true
                };

                await _localDbService.GetConnection().InsertAsync(relationship);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateParentChildRelationshipAsync(ParentChildModel relationship)
        {
            try
            {
                await _localDbService.GetConnection().UpdateAsync(relationship);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RemoveParentChildRelationshipAsync(int parentId, int childId)
        {
            try
            {
                var relationship = await _localDbService.GetConnection()
                    .Table<ParentChildModel>()
                    .Where(pc => pc.ParentId == parentId && pc.ChildId == childId && pc.IsActive)
                    .FirstOrDefaultAsync();

                if (relationship != null)
                {
                    relationship.IsActive = false;
                    await _localDbService.GetConnection().UpdateAsync(relationship);
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }





        #endregion

        #region Assignment and Grade Management

        public async Task<IEnumerable<AssignmentModel>> GetAllAssignmentsAsync()
        {
            return await _localDbService.GetConnection().Table<AssignmentModel>().ToListAsync();
        }

        public async Task<IEnumerable<AssignmentModel>> GetAssignmentsByStudentAsync(int studentId)
        {
            return await _localDbService.GetConnection()
                .Table<AssignmentModel>()
                .Where(a => a.StudentId == studentId)
                .ToListAsync();
        }

        public async Task<IEnumerable<GradeModel>> GetAllGradesAsync()
        {
            return await _localDbService.GetConnection().Table<GradeModel>().ToListAsync();
        }

        public async Task<IEnumerable<GradeModel>> GetGradesByStudentAsync(int studentId)
        {
            return await _localDbService.GetConnection()
                .Table<GradeModel>()
                .Where(g => g.StudentId == studentId)
                .ToListAsync();
        }

        public async Task<bool> DeleteAssignmentAsync(int assignmentId)
        {
            var assignment = await _localDbService.GetConnection().FindAsync<AssignmentModel>(assignmentId);
            if (assignment == null) return false;

            await _localDbService.GetConnection().DeleteAsync(assignment);
            return true;
        }

        public async Task<bool> DeleteGradeAsync(int gradeId)
        {
            var grade = await _localDbService.GetConnection().FindAsync<GradeModel>(gradeId);
            if (grade == null) return false;

            await _localDbService.GetConnection().DeleteAsync(grade);
            return true;
        }

        #endregion
    }
}
