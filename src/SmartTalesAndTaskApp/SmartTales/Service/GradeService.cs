using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SmartTales.Model;
using SmartTales.Repository.IRepository;
using SmartTales.Data;

namespace SmartTales.Service
{
    public class GradeService
    {
        private readonly IGradeRepository _gradeRepository;
        private readonly IAssignmentRepository _assignmentRepository;
        private readonly IUserRepository _userRepository;

        // Event that components can subscribe to for notifications when grades change
        public event Action OnGradesChanged;

        public GradeService(IGradeRepository gradeRepository, IAssignmentRepository assignmentRepository, IUserRepository userRepository)
        {
            _gradeRepository = gradeRepository ?? throw new ArgumentNullException(nameof(gradeRepository));
            _assignmentRepository = assignmentRepository ?? throw new ArgumentNullException(nameof(assignmentRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        // Method to notify subscribers that grades have changed
        private void NotifyGradesChanged()
        {
            OnGradesChanged?.Invoke();
        }

        // Create or update a grade
        public async Task<GradeModel> SaveGradeAsync(GradeModel grade)
        {
            GradeModel result;
            
            // Check if grade already exists for this student and assignment
            var existingGrade = await _gradeRepository.GetGradeByStudentAndAssignmentAsync(grade.StudentId, grade.AssignmentId);
            
            if (existingGrade != null)
            {
                // Update existing grade
                existingGrade.NumericalGrade = grade.NumericalGrade;
                existingGrade.MaxPoints = grade.MaxPoints;
                existingGrade.LetterGrade = grade.LetterGrade;
                existingGrade.Feedback = grade.Feedback;
                existingGrade.TeacherId = grade.TeacherId;
                existingGrade.GradedDate = DateTime.Now;
                
                result = await _gradeRepository.UpdateAsync(existingGrade);
            }
            else
            {
                // Create new grade
                result = await _gradeRepository.CreateAsync(grade);
            }

            // Notify subscribers that grades have changed
            NotifyGradesChanged();

            return result;
        }

        // Get grades for a specific student with assignment details
        public async Task<List<GradeViewModel>> GetStudentGradesWithDetailsAsync(int studentId)
        {
            var grades = await _gradeRepository.GetGradesByStudentAsync(studentId);
            var allAssignments = await _assignmentRepository.GetAllAsync();
            var allUsers = await _userRepository.GetAllAsync();
            
            var student = allUsers.FirstOrDefault(u => u.Id == studentId);
            var teachers = allUsers.Where(u => u.Role == "Teacher").ToDictionary(u => u.Id, u => u);

            var gradeViewModels = new List<GradeViewModel>();

            foreach (var grade in grades)
            {
                var assignment = allAssignments.FirstOrDefault(a => a.AssignmentId == grade.AssignmentId);
                if (assignment == null) continue;

                var teacher = grade.TeacherId.HasValue && teachers.ContainsKey(grade.TeacherId.Value) 
                    ? teachers[grade.TeacherId.Value] 
                    : null;

                var gradeViewModel = new GradeViewModel
                {
                    GradeId = grade.GradeId,
                    StudentId = grade.StudentId,
                    AssignmentId = grade.AssignmentId,
                    TeacherId = grade.TeacherId,
                    NumericalGrade = grade.NumericalGrade,
                    MaxPoints = grade.MaxPoints,
                    LetterGrade = grade.LetterGrade,
                    Feedback = grade.Feedback,
                    GradedDate = grade.GradedDate,

                    // Student information
                    StudentName = student != null ? $"{student.FirstName} {student.LastName}" : "Unknown Student",
                    StudentEmail = student?.Email ?? "",
                    GradeLevel = student?.GradeLevel ?? "",

                    // Assignment information
                    AssignmentTitle = assignment.Title,
                    AssignmentDescription = assignment.Description,
                    Class = assignment.Class,
                    DueDate = assignment.DueDate,
                    AssignmentPoints = assignment.Points,
                    AssignmentType = assignment.AssignmentType,

                    // Teacher information
                    TeacherName = teacher != null ? $"{teacher.FirstName} {teacher.LastName}" : assignment.TeacherName,

                    // Submission information
                    SubmissionDate = assignment.SubmissionDate,
                    IsSubmitted = assignment.IsSubmitted
                };

                gradeViewModels.Add(gradeViewModel);
            }

            return gradeViewModels.OrderByDescending(g => g.GradedDate).ToList();
        }

        // Get all submissions with grading status for teacher dashboard
        public async Task<List<GradeViewModel>> GetSubmissionsWithGradingStatusAsync()
        {
            var allAssignments = await _assignmentRepository.GetAllAsync();
            var submittedAssignments = allAssignments.Where(a => a.IsSubmitted && a.StudentId.HasValue);
            var allUsers = await _userRepository.GetAllAsync();
            var students = allUsers.Where(u => u.Role == "Kid").ToDictionary(u => u.Id, u => u);
            var teachers = allUsers.Where(u => u.Role == "Teacher").ToDictionary(u => u.Id, u => u);

            var gradeViewModels = new List<GradeViewModel>();

            foreach (var assignment in submittedAssignments)
            {
                if (!assignment.StudentId.HasValue || !students.ContainsKey(assignment.StudentId.Value))
                    continue;

                var student = students[assignment.StudentId.Value];
                
                // Check if this assignment has been graded
                var grade = await _gradeRepository.GetGradeByStudentAndAssignmentAsync(assignment.StudentId.Value, assignment.AssignmentId);

                var gradeViewModel = new GradeViewModel
                {
                    GradeId = grade?.GradeId ?? 0,
                    StudentId = assignment.StudentId.Value,
                    AssignmentId = assignment.AssignmentId,
                    TeacherId = grade?.TeacherId,
                    NumericalGrade = grade?.NumericalGrade,
                    MaxPoints = grade?.MaxPoints ?? (decimal)assignment.Points,
                    LetterGrade = grade?.LetterGrade ?? "",
                    Feedback = grade?.Feedback ?? "",
                    GradedDate = grade?.GradedDate,

                    // Student information
                    StudentName = $"{student.FirstName} {student.LastName}",
                    StudentEmail = student.Email,
                    StudentInitials = $"{student.FirstName.FirstOrDefault()}{student.LastName.FirstOrDefault()}",
                    GradeLevel = student.GradeLevel ?? "",

                    // Assignment information
                    AssignmentTitle = assignment.Title,
                    AssignmentDescription = assignment.Description,
                    Class = assignment.Class,
                    DueDate = assignment.DueDate,
                    AssignmentPoints = assignment.Points,
                    AssignmentType = assignment.AssignmentType,

                    // Teacher information
                    TeacherName = assignment.TeacherName,

                    // Submission information
                    SubmissionDate = assignment.SubmissionDate,
                    IsSubmitted = assignment.IsSubmitted,
                    SubmissionNotes = assignment.SubmissionNotes ?? "",
                    SubmittedFiles = assignment.SubmittedFiles ?? new List<string>()
                };

                gradeViewModels.Add(gradeViewModel);
            }

            return gradeViewModels.OrderBy(g => g.IsGraded).ThenByDescending(g => g.SubmissionDate).ToList();
        }

        // Get grade statistics for a student
        public async Task<StudentGradeStats> GetStudentGradeStatsAsync(int studentId)
        {
            var grades = await _gradeRepository.GetGradesByStudentAsync(studentId);
            var averageGrade = await _gradeRepository.GetAverageGradeForStudentAsync(studentId);
            var gradedCount = await _gradeRepository.GetGradedAssignmentsCountForStudentAsync(studentId);

            return new StudentGradeStats
            {
                StudentId = studentId,
                TotalGradedAssignments = gradedCount,
                AverageGrade = averageGrade,
                HighestGrade = grades.Where(g => g.PercentageGrade.HasValue).Max(g => g.PercentageGrade),
                LowestGrade = grades.Where(g => g.PercentageGrade.HasValue).Min(g => g.PercentageGrade),
                RecentGrades = grades.Take(5).ToList()
            };
        }

        // Delete a grade
        public async Task<bool> DeleteGradeAsync(int gradeId)
        {
            var result = await _gradeRepository.DeleteAsync(gradeId);
            if (result)
            {
                NotifyGradesChanged();
            }
            return result;
        }

        // Get grade by ID
        public async Task<GradeModel?> GetGradeByIdAsync(int gradeId)
        {
            return await _gradeRepository.GetAsync(gradeId);
        }

        // Get grade for specific student and assignment
        public async Task<GradeModel?> GetGradeByStudentAndAssignmentAsync(int studentId, int assignmentId)
        {
            return await _gradeRepository.GetGradeByStudentAndAssignmentAsync(studentId, assignmentId);
        }
    }

    // Helper class for student grade statistics
    public class StudentGradeStats
    {
        public int StudentId { get; set; }
        public int TotalGradedAssignments { get; set; }
        public decimal? AverageGrade { get; set; }
        public decimal? HighestGrade { get; set; }
        public decimal? LowestGrade { get; set; }
        public List<GradeModel> RecentGrades { get; set; } = new List<GradeModel>();
    }
}
