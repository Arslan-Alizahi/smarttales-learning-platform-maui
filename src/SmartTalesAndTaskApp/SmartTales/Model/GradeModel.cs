using System;
using SQLite;

namespace SmartTales.Model
{
    [Table("Grade")]
    public class GradeModel
    {
        [PrimaryKey, AutoIncrement]
        [Column("GradeId")]
        public int GradeId { get; set; }

        [Column("StudentId")]
        public int StudentId { get; set; }

        [Column("AssignmentId")]
        public int AssignmentId { get; set; }

        [Column("TeacherId")]
        public int? TeacherId { get; set; }

        [Column("NumericalGrade")]
        public decimal? NumericalGrade { get; set; }

        [Column("MaxPoints")]
        public decimal MaxPoints { get; set; } = 100;

        [Column("LetterGrade")]
        public string LetterGrade { get; set; } = string.Empty;

        [Column("Feedback")]
        public string Feedback { get; set; } = string.Empty;

        [Column("GradedDate")]
        public DateTime GradedDate { get; set; } = DateTime.Now;

        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column("UpdatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Computed properties for display
        [Ignore]
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

        [Ignore]
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

        [Ignore]
        public string PercentageDisplay
        {
            get
            {
                var percentage = PercentageGrade;
                return percentage.HasValue ? $"{percentage:F1}%" : "N/A";
            }
        }

        [Ignore]
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

        [Ignore]
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

        [Ignore]
        public string GradedTimeAgo
        {
            get
            {
                var timeSpan = DateTime.Now - GradedDate;
                if (timeSpan.TotalDays >= 1)
                    return $"{(int)timeSpan.TotalDays} day(s) ago";
                else if (timeSpan.TotalHours >= 1)
                    return $"{(int)timeSpan.TotalHours} hour(s) ago";
                else
                    return $"{(int)timeSpan.TotalMinutes} minute(s) ago";
            }
        }
    }
}
