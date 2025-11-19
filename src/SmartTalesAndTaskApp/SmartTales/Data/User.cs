using SQLite;
using System;

namespace SmartTales.Data
{
    [Table("User")]
    public class User
    {
        [PrimaryKey, AutoIncrement]
        [Column("ID")]
        public int Id { get; set; }

        // Common properties for all users
        [Column("First Name")]
        public string FirstName { get; set; }

        [Column("Last Name")]
        public string LastName { get; set; }

        [Column("Email Address")]
        public string Email { get; set; }

        [Column("Phone Number")]
        public string PhoneNumber { get; set; }

        [Column("Password")]
        public string Password { get; set; }

        [Column("Role")]
        public string Role { get; set; }

        // Properties specific to Kids
        [Column("Grade Level")]
        public string GradeLevel { get; set; }

        // Properties for Kid Dashboard
        [Column("Progress Percentage")]
        public int? ProgressPercentage { get; set; }

        [Column("Completed Assignments")]
        public int? CompletedAssignments { get; set; }

        [Column("Pending Assignments")]
        public int? PendingAssignments { get; set; }

        [Column("Total Points")]
        public int? TotalPoints { get; set; }

        // Properties specific to Parents
        [Column("Address")]
        public string Address { get; set; }

        // Properties specific to Teachers
        [Column("Specialization")]
        public string Specialization { get; set; }

        [Column("Years Of Experience")]
        public int? YearsOfExperience { get; set; }
    }
}
