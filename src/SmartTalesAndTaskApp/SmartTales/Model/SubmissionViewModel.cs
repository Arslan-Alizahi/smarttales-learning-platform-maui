using System;
using System.Collections.Generic;

namespace SmartTales.Model
{
    public class SubmissionViewModel
    {
        public int AssignmentId { get; set; }
        public string AssignmentTitle { get; set; } = string.Empty;
        public string AssignmentDescription { get; set; } = string.Empty;
        public string Class { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public int Points { get; set; }
        public string TeacherName { get; set; } = string.Empty;
        
        // Student information
        public int? StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentInitials { get; set; } = string.Empty;
        public string StudentEmail { get; set; } = string.Empty;
        public string GradeLevel { get; set; } = string.Empty;
        
        // Submission details
        public DateTime? SubmissionDate { get; set; }
        public List<string> SubmittedFiles { get; set; } = new List<string>();
        public string SubmissionNotes { get; set; } = string.Empty;
        
        // Display helpers
        public string SubmissionTimeFormatted => SubmissionDate?.ToString("MMM dd, yyyy h:mm tt") ?? "Not submitted";
        public string SubmissionTimeAgo
        {
            get
            {
                if (!SubmissionDate.HasValue) return "Not submitted";
                
                var timeSpan = DateTime.Now - SubmissionDate.Value;
                if (timeSpan.TotalDays >= 1)
                    return $"{(int)timeSpan.TotalDays} day(s) ago";
                else if (timeSpan.TotalHours >= 1)
                    return $"{(int)timeSpan.TotalHours} hour(s) ago";
                else
                    return $"{(int)timeSpan.TotalMinutes} minute(s) ago";
            }
        }
        
        public bool HasFiles => SubmittedFiles != null && SubmittedFiles.Count > 0;
        public int FileCount => SubmittedFiles?.Count ?? 0;
    }
}
