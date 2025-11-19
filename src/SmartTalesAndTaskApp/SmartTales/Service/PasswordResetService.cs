using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SmartTales.Model;
using SmartTales.Repository.IRepository;
using SmartTales.Data;
using BCrypt.Net;

namespace SmartTales.Service
{
    public class PasswordResetService
    {
        private readonly IPasswordResetRepository _passwordResetRepository;
        private readonly IUserRepository _userRepository;
        private readonly TwilioSMSService _twilioSMSService;
        private readonly AdminService _adminService;

        public PasswordResetService(
            IPasswordResetRepository passwordResetRepository,
            IUserRepository userRepository,
            TwilioSMSService twilioSMSService,
            AdminService adminService)
        {
            _passwordResetRepository = passwordResetRepository;
            _userRepository = userRepository;
            _twilioSMSService = twilioSMSService;
            _adminService = adminService;
        }

        // Create password reset request
        public async Task<bool> CreatePasswordResetRequestAsync(int userId)
        {
            try
            {
                var user = await _userRepository.GetAsync(userId);
                if (user == null)
                    return false;

                // Check if there's already a pending request for this user
                var existingRequest = await _passwordResetRepository.GetPendingRequestByUserIdAsync(userId);
                if (existingRequest != null)
                {
                    // Update existing request timestamp
                    existingRequest.RequestDateTime = DateTime.Now;
                    await _passwordResetRepository.UpdateAsync(existingRequest);
                    return true;
                }

                // Create new password reset request
                var resetRequest = new PasswordResetRequestModel
                {
                    UserId = userId,
                    UserName = $"{user.FirstName} {user.LastName}",
                    UserEmail = user.Email,
                    UserPhoneNumber = user.PhoneNumber ?? "",
                    UserRole = user.Role,
                    RequestDateTime = DateTime.Now,
                    Status = "Pending"
                };

                await _passwordResetRepository.CreateAsync(resetRequest);

                // Log admin notification
                await _adminService.LogAdminActionAsync(
                    0, // System generated
                    "PASSWORD_RESET_REQUEST",
                    "User",
                    userId,
                    null,
                    $"Password reset requested by {user.FirstName} {user.LastName} ({user.Role})",
                    true,
                    $"User ID: {userId}, Email: {user.Email}"
                );

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating password reset request: {ex.Message}");
                return false;
            }
        }

        // Get all pending password reset requests
        public async Task<List<PasswordResetRequestModel>> GetPendingPasswordResetRequestsAsync()
        {
            try
            {
                return await _passwordResetRepository.GetPendingRequestsAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting pending requests: {ex.Message}");
                return new List<PasswordResetRequestModel>();
            }
        }

        // Get all password reset requests (for admin dashboard)
        public async Task<List<PasswordResetRequestModel>> GetAllPasswordResetRequestsAsync()
        {
            try
            {
                return await _passwordResetRepository.GetAllRequestsAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting all requests: {ex.Message}");
                return new List<PasswordResetRequestModel>();
            }
        }

        // Process password reset by admin
        public async Task<PasswordResetResult> ProcessPasswordResetAsync(int requestId, int adminId, string newPassword = "")
        {
            try
            {
                var request = await _passwordResetRepository.GetAsync(requestId);
                if (request == null || request.Status != "Pending")
                    return new PasswordResetResult { Success = false, Message = "Request not found or already processed" };

                var user = await _userRepository.GetAsync(request.UserId);
                if (user == null)
                    return new PasswordResetResult { Success = false, Message = "User not found" };

                // Generate new password if not provided
                if (string.IsNullOrEmpty(newPassword))
                {
                    newPassword = GenerateSecurePassword();
                }

                // Update user password
                user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
                await _userRepository.UpdateAsync(user);

                // Send SMS with new password
                var smsResponse = await _twilioSMSService.SendPasswordResetSMSAsync(
                    request.UserPhoneNumber,
                    newPassword,
                    request.UserName,
                    request.UserRole
                );

                // Update request status - Mark as completed even if SMS fails
                request.Status = "Completed";
                request.AdminId = adminId;
                request.ProcessedDateTime = DateTime.Now;
                request.NewPasswordSent = smsResponse.Success;
                request.SMSDeliveryStatus = smsResponse.Success ? "Sent" : "Failed";
                request.TwilioMessageSid = smsResponse.MessageSid;
                request.Notes = smsResponse.Success ? "Password reset and SMS sent successfully" : $"Password reset completed but SMS failed: {smsResponse.Message}. Error: {smsResponse.ErrorDetails}";

                await _passwordResetRepository.UpdateAsync(request);

                // Log admin action
                await _adminService.LogAdminActionAsync(
                    adminId,
                    "PASSWORD_RESET_PROCESSED",
                    "PasswordResetRequest",
                    request.RequestId,
                    null,
                    $"Password reset processed for {request.UserName} ({request.UserRole})",
                    true, // Always log as success since password was reset
                    $"SMS Status: {request.SMSDeliveryStatus}, Message SID: {request.TwilioMessageSid}"
                );

                // Return detailed result
                return new PasswordResetResult
                {
                    Success = true,
                    SMSDelivered = smsResponse.Success,
                    Message = smsResponse.Success ? "Password reset and SMS sent successfully" : "Password reset completed but SMS delivery failed",
                    SMSError = smsResponse.Success ? null : smsResponse.Message
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing password reset: {ex.Message}");
                return new PasswordResetResult { Success = false, Message = $"Error: {ex.Message}" };
            }
        }

        // Cancel password reset request
        public async Task<bool> CancelPasswordResetRequestAsync(int requestId, int adminId, string reason = "")
        {
            try
            {
                var request = await _passwordResetRepository.GetAsync(requestId);
                if (request == null)
                    return false;

                request.Status = "Cancelled";
                request.AdminId = adminId;
                request.ProcessedDateTime = DateTime.Now;
                request.Notes = $"Cancelled by admin. Reason: {reason}";

                await _passwordResetRepository.UpdateAsync(request);

                // Log admin action
                await _adminService.LogAdminActionAsync(
                    adminId,
                    "PASSWORD_RESET_CANCELLED",
                    "PasswordResetRequest",
                    request.RequestId,
                    null,
                    $"Password reset cancelled for {request.UserName} ({request.UserRole})",
                    true,
                    $"Reason: {reason}"
                );

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cancelling password reset request: {ex.Message}");
                return false;
            }
        }

        // Generate secure password
        private string GenerateSecurePassword()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789";
            const string specialChars = "!@#$%";
            
            var random = new Random();
            var password = new char[8];
            
            // Ensure at least one uppercase, one lowercase, one number, and one special char
            password[0] = chars[random.Next(0, 26)]; // Uppercase
            password[1] = chars[random.Next(26, 52)]; // Lowercase
            password[2] = chars[random.Next(52, chars.Length)]; // Number
            password[3] = specialChars[random.Next(specialChars.Length)]; // Special char
            
            // Fill remaining positions
            for (int i = 4; i < password.Length; i++)
            {
                password[i] = chars[random.Next(chars.Length)];
            }
            
            // Shuffle the password
            for (int i = password.Length - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (password[i], password[j]) = (password[j], password[i]);
            }
            
            return new string(password);
        }

        // Get request count for dashboard
        public async Task<int> GetPendingRequestCountAsync()
        {
            try
            {
                var requests = await GetPendingPasswordResetRequestsAsync();
                return requests.Count;
            }
            catch
            {
                return 0;
            }
        }
    }

    // Result class for password reset operations
    public class PasswordResetResult
    {
        public bool Success { get; set; }
        public bool SMSDelivered { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? SMSError { get; set; }
    }
}
