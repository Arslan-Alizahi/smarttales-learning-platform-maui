using SmartTales.Model;
using SmartTales.Data;

namespace SmartTales.Repository.IRepository
{
    public interface IAdminRepository
    {
        // Admin User Management
        Task<AdminUserModel> CreateAdminAsync(AdminUserModel admin);
        Task<bool> DeleteAdminAsync(int id);
        Task<AdminUserModel?> GetAdminAsync(int id);
        Task<IEnumerable<AdminUserModel>> GetAllAdminsAsync();
        Task<AdminUserModel?> UpdateAdminAsync(AdminUserModel admin);
        Task<AdminUserModel?> GetAdminByUsernameAsync(string username);
        Task<AdminUserModel?> GetAdminByEmailAsync(string email);
        Task<AdminUserModel?> LoginAsync(string username, string password);
        Task<bool> UpdateLastLoginAsync(int adminId);
        Task<bool> ActivateAdminAsync(int adminId, int updatedBy);
        Task<bool> DeactivateAdminAsync(int adminId, int updatedBy);
        Task<bool> ResetPasswordAsync(int adminId, string newPassword, int updatedBy);

        // User Account Management (for regular users)
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<IEnumerable<User>> GetUsersByRoleAsync(string role);
        Task<IEnumerable<User>> SearchUsersAsync(string searchTerm);
        Task<User?> GetUserAsync(int id);
        Task<User?> GetUserByEmailAsync(string email);
        Task<User> CreateUserAsync(User user);
        Task<User?> UpdateUserAsync(User user);
        Task<bool> DeleteUserAsync(int id);
        Task<bool> ActivateUserAsync(int userId);
        Task<bool> DeactivateUserAsync(int userId);
        Task<bool> ResetUserPasswordAsync(int userId, string newPassword);
        Task<IEnumerable<User>> GetUsersWithPaginationAsync(int page, int pageSize);
        Task<int> GetTotalUsersCountAsync();
        Task<int> GetUsersCountByRoleAsync(string role);

        // Bulk Operations
        Task<bool> BulkDeleteUsersAsync(IEnumerable<int> userIds);
        Task<bool> BulkUpdateUserRoleAsync(IEnumerable<int> userIds, string newRole);
        Task<bool> BulkActivateUsersAsync(IEnumerable<int> userIds);
        Task<bool> BulkDeactivateUsersAsync(IEnumerable<int> userIds);

        // Audit Logging
        Task<AdminAuditLogModel> CreateAuditLogAsync(AdminAuditLogModel auditLog);
        Task<IEnumerable<AdminAuditLogModel>> GetAuditLogsAsync(int page, int pageSize);
        Task<IEnumerable<AdminAuditLogModel>> GetAuditLogsByAdminAsync(int adminId, int page, int pageSize);
        Task<IEnumerable<AdminAuditLogModel>> GetAuditLogsByEntityAsync(string entityType, int entityId);
        Task<IEnumerable<AdminAuditLogModel>> GetAuditLogsByActionAsync(string action);
        Task<IEnumerable<AdminAuditLogModel>> GetAuditLogsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<int> GetTotalAuditLogsCountAsync();

        // Statistics and Analytics
        Task<Dictionary<string, int>> GetUserStatsByRoleAsync();
        Task<Dictionary<string, int>> GetUserRegistrationStatsAsync(int days = 30);
        Task<Dictionary<string, int>> GetAdminActivityStatsAsync(int days = 30);
        Task<IEnumerable<User>> GetRecentlyRegisteredUsersAsync(int count = 10);
        Task<IEnumerable<AdminAuditLogModel>> GetRecentAdminActionsAsync(int count = 10);

        // Parent-Child Relationship Management
        Task<IEnumerable<ParentChildModel>> GetAllParentChildRelationshipsAsync();
        Task<IEnumerable<ParentChildModel>> GetParentChildRelationshipsByParentAsync(int parentId);
        Task<IEnumerable<ParentChildModel>> GetParentChildRelationshipsByChildAsync(int childId);
        Task<bool> CreateParentChildRelationshipAsync(int parentId, int childId);
        Task<bool> UpdateParentChildRelationshipAsync(ParentChildModel relationship);
        Task<bool> RemoveParentChildRelationshipAsync(int parentId, int childId);

        // Assignment and Grade Management
        Task<IEnumerable<AssignmentModel>> GetAllAssignmentsAsync();
        Task<IEnumerable<AssignmentModel>> GetAssignmentsByStudentAsync(int studentId);
        Task<IEnumerable<GradeModel>> GetAllGradesAsync();
        Task<IEnumerable<GradeModel>> GetGradesByStudentAsync(int studentId);
        Task<bool> DeleteAssignmentAsync(int assignmentId);
        Task<bool> DeleteGradeAsync(int gradeId);
    }
}
