using SQLite;
using System;

namespace SmartTales.Model
{
    [Table("AdminAuditLog")]
    public class AdminAuditLogModel
    {
        [PrimaryKey, AutoIncrement]
        [Column("Id")]
        public int Id { get; set; }

        [Column("AdminUserId")]
        public int AdminUserId { get; set; }

        [Column("Action")]
        public string Action { get; set; } = string.Empty;

        [Column("EntityType")]
        public string EntityType { get; set; } = string.Empty; // User, Assignment, Grade, etc.

        [Column("EntityId")]
        public int? EntityId { get; set; }

        [Column("OldValues")]
        public string OldValues { get; set; } = string.Empty; // JSON string

        [Column("NewValues")]
        public string NewValues { get; set; } = string.Empty; // JSON string

        [Column("IPAddress")]
        public string IPAddress { get; set; } = string.Empty;

        [Column("UserAgent")]
        public string UserAgent { get; set; } = string.Empty;

        [Column("Timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.Now;

        [Column("Success")]
        public bool Success { get; set; } = true;

        [Column("ErrorMessage")]
        public string ErrorMessage { get; set; } = string.Empty;

        [Column("AdditionalData")]
        public string AdditionalData { get; set; } = string.Empty; // JSON string for extra context

        // Navigation properties (not stored in database)
        [Ignore]
        public string ActionDescription => Action switch
        {
            "CREATE_USER" => "Created User",
            "UPDATE_USER" => "Updated User",
            "DELETE_USER" => "Deleted User",
            "RESET_PASSWORD" => "Reset Password",
            "ACTIVATE_USER" => "Activated User",
            "DEACTIVATE_USER" => "Deactivated User",
            "LOGIN" => "Admin Login",
            "LOGOUT" => "Admin Logout",
            "VIEW_USERS" => "Viewed Users",
            "EXPORT_DATA" => "Exported Data",
            "BULK_UPDATE" => "Bulk Update",
            "SYSTEM_CONFIG" => "System Configuration",
            _ => Action
        };

        [Ignore]
        public string EntityDisplayName => EntityType switch
        {
            "User" => "User Account",
            "AdminUser" => "Admin Account",
            "Assignment" => "Assignment",
            "Grade" => "Grade",
            "ParentChild" => "Parent-Child Relationship",
            _ => EntityType
        };
    }
}
