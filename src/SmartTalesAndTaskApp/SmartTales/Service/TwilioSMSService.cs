using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace SmartTales.Service
{
    public class TwilioSMSService
    {
        private readonly HttpClient _httpClient;
        private readonly string _accountSid;
        private readonly string _authToken;
        private readonly string _fromPhoneNumber;

        public TwilioSMSService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            
            // Twilio credentials - replace with your actual credentials
            _accountSid = "YOUR_TWILIO_ACCOUNT_SID";
            _authToken = "YOUR_TWILIO_AUTH_TOKEN";
            _fromPhoneNumber = "YOUR_TWILIO_PHONE_NUMBER";

            // Set up basic authentication for Twilio API
            var authValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_accountSid}:{_authToken}"));
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authValue);
        }

        public async Task<TwilioSMSResponse> SendPasswordResetSMSAsync(string toPhoneNumber, string newPassword, string userName, string userRole)
        {
            try
            {
                // Format phone number (ensure it starts with +)
                if (!toPhoneNumber.StartsWith("+"))
                {
                    // Clean the phone number
                    var cleanNumber = toPhoneNumber.Replace("-", "").Replace("(", "").Replace(")", "").Replace(" ", "");

                    // Check if it's a Pakistani number (starts with 0)
                    if (cleanNumber.StartsWith("0"))
                    {
                        // Pakistani number - add country code +92 and remove leading 0
                        toPhoneNumber = "+92" + cleanNumber.Substring(1);
                    }
                    else if (cleanNumber.Length == 10 || cleanNumber.Length == 11)
                    {
                        // Assume US number if 10-11 digits
                        toPhoneNumber = "+1" + cleanNumber;
                    }
                    else
                    {
                        // Default to US format
                        toPhoneNumber = "+1" + cleanNumber;
                    }
                }

                // Create SMS message
                var messageBody = $"SmartTales Password Reset\n\nHello {userName},\n\nYour password has been reset by the administrator.\n\nNew Password: {newPassword}\n\nPlease login and change your password immediately.\n\nTime: {DateTime.Now:MMM dd, yyyy HH:mm}\n\nSmartTales Support Team";

                Console.WriteLine($"Sending SMS to: {toPhoneNumber}");

                // Prepare form data for Twilio API
                var formData = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("To", toPhoneNumber),
                    new KeyValuePair<string, string>("From", _fromPhoneNumber),
                    new KeyValuePair<string, string>("Body", messageBody)
                };

                var formContent = new FormUrlEncodedContent(formData);

                // Send SMS via Twilio API
                var response = await _httpClient.PostAsync($"https://api.twilio.com/2010-04-01/Accounts/{_accountSid}/Messages.json", formContent);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var twilioResponse = JsonSerializer.Deserialize<TwilioApiResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    return new TwilioSMSResponse
                    {
                        Success = true,
                        MessageSid = twilioResponse?.Sid ?? "",
                        Status = twilioResponse?.Status ?? "sent",
                        Message = "SMS sent successfully",
                        ToPhoneNumber = toPhoneNumber,
                        SentDateTime = DateTime.Now
                    };
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Twilio API Error: {response.StatusCode} - {errorContent}");
                    Console.WriteLine($"Phone number used: {toPhoneNumber}");
                    Console.WriteLine($"Account SID: {_accountSid}");

                    return new TwilioSMSResponse
                    {
                        Success = false,
                        Message = $"Failed to send SMS: {response.StatusCode}",
                        ErrorDetails = errorContent,
                        ToPhoneNumber = toPhoneNumber,
                        SentDateTime = DateTime.Now
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending SMS: {ex.Message}");
                return new TwilioSMSResponse
                {
                    Success = false,
                    Message = $"Exception occurred: {ex.Message}",
                    ErrorDetails = ex.ToString(),
                    ToPhoneNumber = toPhoneNumber,
                    SentDateTime = DateTime.Now
                };
            }
        }

        public async Task<TwilioSMSResponse> SendNotificationSMSAsync(string toPhoneNumber, string message)
        {
            try
            {
                // Format phone number
                if (!toPhoneNumber.StartsWith("+"))
                {
                    // Clean the phone number
                    var cleanNumber = toPhoneNumber.Replace("-", "").Replace("(", "").Replace(")", "").Replace(" ", "");

                    // Check if it's a Pakistani number (starts with 0)
                    if (cleanNumber.StartsWith("0"))
                    {
                        // Pakistani number - add country code +92 and remove leading 0
                        toPhoneNumber = "+92" + cleanNumber.Substring(1);
                    }
                    else if (cleanNumber.Length == 10 || cleanNumber.Length == 11)
                    {
                        // Assume US number if 10-11 digits
                        toPhoneNumber = "+1" + cleanNumber;
                    }
                    else
                    {
                        // Default to US format
                        toPhoneNumber = "+1" + cleanNumber;
                    }
                }

                var formData = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("To", toPhoneNumber),
                    new KeyValuePair<string, string>("From", _fromPhoneNumber),
                    new KeyValuePair<string, string>("Body", message)
                };

                var formContent = new FormUrlEncodedContent(formData);
                var response = await _httpClient.PostAsync($"https://api.twilio.com/2010-04-01/Accounts/{_accountSid}/Messages.json", formContent);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var twilioResponse = JsonSerializer.Deserialize<TwilioApiResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    return new TwilioSMSResponse
                    {
                        Success = true,
                        MessageSid = twilioResponse?.Sid ?? "",
                        Status = twilioResponse?.Status ?? "sent",
                        Message = "SMS sent successfully",
                        ToPhoneNumber = toPhoneNumber,
                        SentDateTime = DateTime.Now
                    };
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return new TwilioSMSResponse
                    {
                        Success = false,
                        Message = $"Failed to send SMS: {response.StatusCode}",
                        ErrorDetails = errorContent,
                        ToPhoneNumber = toPhoneNumber,
                        SentDateTime = DateTime.Now
                    };
                }
            }
            catch (Exception ex)
            {
                return new TwilioSMSResponse
                {
                    Success = false,
                    Message = $"Exception occurred: {ex.Message}",
                    ErrorDetails = ex.ToString(),
                    ToPhoneNumber = toPhoneNumber,
                    SentDateTime = DateTime.Now
                };
            }
        }
    }

    // Response models
    public class TwilioSMSResponse
    {
        public bool Success { get; set; }
        public string MessageSid { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string ErrorDetails { get; set; } = string.Empty;
        public string ToPhoneNumber { get; set; } = string.Empty;
        public DateTime SentDateTime { get; set; }
    }

    public class TwilioApiResponse
    {
        public string Sid { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string To { get; set; } = string.Empty;
        public string From { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
    }
}
