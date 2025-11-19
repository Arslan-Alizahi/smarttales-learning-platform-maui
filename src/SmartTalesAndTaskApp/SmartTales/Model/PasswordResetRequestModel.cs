using SQLite;
using System;

namespace SmartTales.Model
{
    [Table("PasswordResetRequest")]
    public class PasswordResetRequestModel
    {
        [PrimaryKey, AutoIncrement]
        [Column("RequestId")]
        public int RequestId { get; set; }

        [Column("UserId")]
        public int UserId { get; set; }

        [Column("UserName")]
        public string UserName { get; set; } = string.Empty;

        [Column("UserEmail")]
        public string UserEmail { get; set; } = string.Empty;

        [Column("UserPhoneNumber")]
        public string UserPhoneNumber { get; set; } = string.Empty;

        [Column("UserRole")]
        public string UserRole { get; set; } = string.Empty;

        [Column("RequestDateTime")]
        public DateTime RequestDateTime { get; set; } = DateTime.Now;

        [Column("Status")]
        public string Status { get; set; } = "Pending"; // Pending, Completed, Cancelled

        [Column("AdminId")]
        public int? AdminId { get; set; }

        [Column("ProcessedDateTime")]
        public DateTime? ProcessedDateTime { get; set; }

        [Column("NewPasswordSent")]
        public bool NewPasswordSent { get; set; } = false;

        [Column("SMSDeliveryStatus")]
        public string SMSDeliveryStatus { get; set; } = "Not Sent"; // Not Sent, Sent, Delivered, Failed

        [Column("TwilioMessageSid")]
        public string TwilioMessageSid { get; set; } = string.Empty;

        [Column("Notes")]
        public string Notes { get; set; } = string.Empty;

        // Display properties
        [Ignore]
        public string RequestTimeAgo
        {
            get
            {
                var timeSpan = DateTime.Now - RequestDateTime;
                if (timeSpan.TotalMinutes < 1)
                    return "Just now";
                if (timeSpan.TotalMinutes < 60)
                    return $"{(int)timeSpan.TotalMinutes} minutes ago";
                if (timeSpan.TotalHours < 24)
                    return $"{(int)timeSpan.TotalHours} hours ago";
                return $"{(int)timeSpan.TotalDays} days ago";
            }
        }

        [Ignore]
        public string StatusBadgeClass
        {
            get
            {
                return Status switch
                {
                    "Pending" => "badge-warning",
                    "Completed" => "badge-success",
                    "Cancelled" => "badge-danger",
                    _ => "badge-secondary"
                };
            }
        }

        [Ignore]
        public string UserDisplayName => $"{UserName} ({UserRole})";
    }
}
