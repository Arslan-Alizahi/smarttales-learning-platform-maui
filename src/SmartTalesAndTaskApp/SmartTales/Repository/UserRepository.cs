using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SmartTales.Data;
using SmartTales.Repository.IRepository;

namespace SmartTales.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly LocalDbService _localDbService;

        public UserRepository(LocalDbService localDbService)
        {
            _localDbService = localDbService ?? throw new ArgumentNullException(nameof(localDbService));
        }

        public async Task<User> CreateAsync(User obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            await _localDbService.GetConnection().InsertAsync(obj);
            return obj;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var obj = await _localDbService.GetConnection().FindAsync<User>(id);
            if (obj == null) return false;

            await _localDbService.GetConnection().DeleteAsync(obj);
            return true;
        }

        public async Task<User?> GetAsync(int id)
        {
            return await _localDbService.GetConnection().FindAsync<User>(id);
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _localDbService.GetConnection().Table<User>().ToListAsync();
        }

        public async Task<User?> UpdateAsync(User obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            var objFromDb = await _localDbService.GetConnection().FindAsync<User>(obj.Id);
            if (objFromDb == null) return null;

            // Update fields based on your schema
            objFromDb.FirstName = obj.FirstName;
            objFromDb.LastName = obj.LastName;
            objFromDb.Email = obj.Email;
            objFromDb.PhoneNumber = obj.PhoneNumber;
            objFromDb.Role = obj.Role;
            objFromDb.Password = obj.Password; // Assuming password is already hashed
            objFromDb.GradeLevel = obj.GradeLevel;
            objFromDb.Address = obj.Address;
            objFromDb.Specialization = obj.Specialization;
            objFromDb.YearsOfExperience = obj.YearsOfExperience;

            // Update dashboard specific fields
            objFromDb.ProgressPercentage = obj.ProgressPercentage;
            objFromDb.CompletedAssignments = obj.CompletedAssignments;
            objFromDb.PendingAssignments = obj.PendingAssignments;
            objFromDb.TotalPoints = obj.TotalPoints;

            await _localDbService.GetConnection().UpdateAsync(objFromDb);
            return objFromDb;
        }

        public async Task<IEnumerable<User>> GetUsersByRoleAsync(string role)
        {
            return await _localDbService.GetConnection()
                .Table<User>()
                .Where(u => u.Role == role)
                .ToListAsync();
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _localDbService.GetConnection()
                .Table<User>()
                .Where(u => u.Email == email)
                .FirstOrDefaultAsync();
        }

        public async Task<User?> GetKidDashboardDetailsAsync(int kidId)
        {
            try
            {
                // Try to get the exact user by ID first
                var kid = await _localDbService.GetConnection().FindAsync<User>(kidId);

                if (kid != null)
                {
                    // Don't filter by Role here - we want the exact user that was requested
                    // Initialize dashboard metrics to 0 if they're null
                    kid.ProgressPercentage ??= 0;
                    kid.CompletedAssignments ??= 0;
                    kid.PendingAssignments ??= 0;
                    kid.TotalPoints ??= 0;

                    Console.WriteLine($"Found exact user with ID {kidId}: {kid.FirstName} {kid.LastName}");
                    return kid;
                }

                // If requested ID is not found, log it and return null
                Console.WriteLine($"User with ID {kidId} not found");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetKidDashboardDetailsAsync: {ex.Message}");
                return null;
            }
        }

        public async Task<User?> LoginAsync(string email, string password)
        {
            var user = await _localDbService.GetConnection()
                .Table<User>()
                .Where(u => u.Email == email)
                .FirstOrDefaultAsync();

            if (user != null)
            {
                try
                {
                    // Try to verify hashed password
                    if (BCrypt.Net.BCrypt.Verify(password, user.Password))
                    {
                        return user;
                    }
                }
                catch (BCrypt.Net.SaltParseException)
                {
                    // If password was stored as plain text (unhashed)
                    if (password == user.Password)
                    {
                        // Update the password with proper hash for future logins
                        user.Password = BCrypt.Net.BCrypt.HashPassword(password);
                        await _localDbService.GetConnection().UpdateAsync(user);
                        return user;
                    }
                }
            }

            return null;
        }

        public async Task<bool> UpdateKidDashboardMetricsAsync(int kidId, int? progressPercentage = null,
            int? completedAssignments = null, int? pendingAssignments = null, int? totalPoints = null)
        {
            try
            {
                var kid = await _localDbService.GetConnection().FindAsync<User>(kidId);

                if (kid != null && kid.Role == "Kid")
                {
                    if (progressPercentage.HasValue)
                        kid.ProgressPercentage = progressPercentage;

                    if (completedAssignments.HasValue)
                        kid.CompletedAssignments = completedAssignments;

                    if (pendingAssignments.HasValue)
                        kid.PendingAssignments = pendingAssignments;

                    if (totalPoints.HasValue)
                        kid.TotalPoints = totalPoints;

                    await _localDbService.GetConnection().UpdateAsync(kid);
                    Console.WriteLine($"Updated dashboard metrics for kid: {kid.FirstName} {kid.LastName}");
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating kid metrics: {ex.Message}");
                return false;
            }
        }

        public async Task<IEnumerable<User>> GetChildrenByParentIdAsync(int parentId)
        {
            try
            {
                var query = @"
                    SELECT u.* FROM User u
                    INNER JOIN ParentChild pc ON u.ID = pc.ChildId
                    WHERE pc.ParentId = ? AND pc.IsActive = 1 AND u.Role = 'Kid'";

                return await _localDbService.GetConnection().QueryAsync<User>(query, parentId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting children by parent ID: {ex.Message}");
                return new List<User>();
            }
        }
    }
}
