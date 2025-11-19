using SQLite;
using System;

namespace SmartTales.Model
{
    [Table("AdminUser")]
    public class AdminUserModel
    {
        [PrimaryKey, AutoIncrement]
        [Column("Id")]
        public int Id { get; set; }

        [Column("Username")]
        public string Username { get; set; } = string.Empty;

        [Column("Email")]
        public string Email { get; set; } = string.Empty;

        [Column("FirstName")]
        public string FirstName { get; set; } = string.Empty;

        [Column("LastName")]
        public string LastName { get; set; } = string.Empty;

        [Column("Password")]
        public string Password { get; set; } = string.Empty;

        [Column("Role")]
        public string Role { get; set; } = "Admin"; // Admin, SuperAdmin

        [Column("IsActive")]
        public bool IsActive { get; set; } = true;

        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column("UpdatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        [Column("LastLoginAt")]
        public DateTime? LastLoginAt { get; set; }

        [Column("CreatedBy")]
        public int? CreatedBy { get; set; }

        [Column("UpdatedBy")]
        public int? UpdatedBy { get; set; }

        // Navigation properties (not stored in database)
        [Ignore]
        public string FullName => $"{FirstName} {LastName}".Trim();

        [Ignore]
        public string DisplayRole => Role switch
        {
            "SuperAdmin" => "Super Administrator",
            "Admin" => "Administrator",
            _ => Role
        };
    }
}
