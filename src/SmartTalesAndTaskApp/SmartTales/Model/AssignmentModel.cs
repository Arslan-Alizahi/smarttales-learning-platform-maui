using System;
using System.Collections.Generic;
using SQLite;
using System.Text.Json;

namespace SmartTales.Model
{
    [Table("Assignment")]
    public class AssignmentModel
    {
        [PrimaryKey, AutoIncrement]
        [Column("AssignmentId")]
        public int AssignmentId { get; set; }

        [Column("Title")]
        public string Title { get; set; } = string.Empty;

        [Column("Description")]
        public string Description { get; set; } = string.Empty;

        [Column("Class")]
        public string Class { get; set; } = string.Empty;

        [Column("DueDate")]
        public DateTime DueDate { get; set; }

        [Column("AssignmentType")]
        public string AssignmentType { get; set; } = string.Empty;

        [Column("Points")]
        public int Points { get; set; }

        [Column("TeacherName")]
        public string TeacherName { get; set; } = string.Empty;

        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column("IsPublished")]
        public bool IsPublished { get; set; } = true;

        // Store attachments as JSON string in SQLite
        [Column("AttachmentsJson")]
        public string AttachmentsJson { get; set; } = "[]";

        // Property to work with List<string> in code
        [Ignore]
        public List<string> Attachments
        {
            get => string.IsNullOrEmpty(AttachmentsJson) ? new List<string>() : JsonSerializer.Deserialize<List<string>>(AttachmentsJson) ?? new List<string>();
            set => AttachmentsJson = JsonSerializer.Serialize(value ?? new List<string>());
        }

        // Submission tracking
        [Column("IsSubmitted")]
        public bool IsSubmitted { get; set; } = false;

        [Column("SubmissionDate")]
        public DateTime? SubmissionDate { get; set; }

        [Column("StudentId")]
        public int? StudentId { get; set; }

        // Store submitted files as JSON string in SQLite
        [Column("SubmittedFilesJson")]
        public string SubmittedFilesJson { get; set; } = "[]";

        // Property to work with List<string> in code
        [Ignore]
        public List<string> SubmittedFiles
        {
            get => string.IsNullOrEmpty(SubmittedFilesJson) ? new List<string>() : JsonSerializer.Deserialize<List<string>>(SubmittedFilesJson) ?? new List<string>();
            set => SubmittedFilesJson = JsonSerializer.Serialize(value ?? new List<string>());
        }

        [Column("SubmissionNotes")]
        public string SubmissionNotes { get; set; } = string.Empty;
    }
}
