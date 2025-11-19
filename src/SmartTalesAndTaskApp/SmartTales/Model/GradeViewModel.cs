using System;
using System.Collections.Generic;

namespace SmartTales.Model
{
    public class GradeViewModel
    {
        // Grade information
        public int GradeId { get; set; }
        public int StudentId { get; set; }
        public int AssignmentId { get; set; }
        public int? TeacherId { get; set; }
        public decimal? NumericalGrade { get; set; }
        public decimal MaxPoints { get; set; } = 100;
        public string LetterGrade { get; set; } = string.Empty;
        public string Feedback { get; set; } = string.Empty;
        public DateTime? GradedDate { get; set; }

        // Student information
        public string StudentName { get; set; } = string.Empty;
        public string StudentEmail { get; set; } = string.Empty;
        public string StudentInitials { get; set; } = string.Empty;
        public string GradeLevel { get; set; } = string.Empty;

        // Assignment information
        public string AssignmentTitle { get; set; } = string.Empty;
        public string AssignmentDescription { get; set; } = string.Empty;
        public string Class { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public int AssignmentPoints { get; set; }
        public string AssignmentType { get; set; } = string.Empty;

        // Teacher information
        public string TeacherName { get; set; } = string.Empty;

        // Submission information
        public DateTime? SubmissionDate { get; set; }
        public bool IsSubmitted { get; set; }
        public string SubmissionNotes { get; set; } = string.Empty;
        public List<string> SubmittedFiles { get; set; } = new List<string>();

        // Display properties
        public string GradeDisplay
        {
            get
            {
                if (NumericalGrade.HasValue)
                {
                    return $"{NumericalGrade:F1}/{MaxPoints}";
                }
                return !string.IsNullOrEmpty(LetterGrade) ? LetterGrade : "Not Graded";
            }
        }

        public decimal? PercentageGrade
        {
            get
            {
                if (NumericalGrade.HasValue && MaxPoints > 0)
                {
                    return Math.Round((NumericalGrade.Value / MaxPoints) * 100, 1);
                }
                return null;
            }
        }

        // Alias for PercentageGrade for compatibility
        public decimal Percentage => PercentageGrade ?? 0;

        public string PercentageDisplay
        {
            get
            {
                var percentage = PercentageGrade;
                return percentage.HasValue ? $"{percentage:F1}%" : "Not Graded";
            }
        }

        public string GradeColor
        {
            get
            {
                var percentage = PercentageGrade;
                if (!percentage.HasValue) return "#6c757d"; // Gray for ungraded

                return percentage.Value switch
                {
                    >= 90 => "#28a745", // Green for A
                    >= 80 => "#17a2b8", // Blue for B
                    >= 70 => "#ffc107", // Yellow for C
                    >= 60 => "#fd7e14", // Orange for D
                    _ => "#dc3545"      // Red for F
                };
            }
        }

        public string AutoLetterGrade
        {
            get
            {
                var percentage = PercentageGrade;
                if (!percentage.HasValue) return "";

                return percentage.Value switch
                {
                    >= 90 => "A",
                    >= 80 => "B",
                    >= 70 => "C",
                    >= 60 => "D",
                    _ => "F"
                };
            }
        }

        public string GradedTimeAgo
        {
            get
            {
                if (GradeId == 0 || !GradedDate.HasValue) return "Not graded";

                var timeSpan = DateTime.Now - GradedDate.Value;
                if (timeSpan.TotalDays >= 1)
                    return $"{(int)timeSpan.TotalDays} day(s) ago";
                else if (timeSpan.TotalHours >= 1)
                    return $"{(int)timeSpan.TotalHours} hour(s) ago";
                else
                    return $"{(int)timeSpan.TotalMinutes} minute(s) ago";
            }
        }

        public string GradedDateFormatted => GradeId > 0 && GradedDate.HasValue ? GradedDate.Value.ToString("MMM dd, yyyy") : "Not graded";

        public bool IsGraded => GradeId > 0;

        public string StatusDisplay => IsGraded ? "Graded" : (IsSubmitted ? "Pending Review" : "Not Submitted");

        public string StatusColor => IsGraded ? "#28a745" : (IsSubmitted ? "#ffc107" : "#6c757d");

        // File handling properties
        public bool HasFiles => SubmittedFiles != null && SubmittedFiles.Count > 0;
        public int FileCount => SubmittedFiles?.Count ?? 0;

        // Additional display properties for compatibility
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

        public string SubmissionTimeFormatted => SubmissionDate?.ToString("MMM dd, yyyy HH:mm") ?? "Not submitted";

        public int Points => AssignmentPoints;
    }
}
