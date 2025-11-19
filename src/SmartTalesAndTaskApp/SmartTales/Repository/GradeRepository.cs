using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SmartTales.Data;
using SmartTales.Model;
using SmartTales.Repository.IRepository;

namespace SmartTales.Repository
{
    public class GradeRepository : IGradeRepository
    {
        private readonly LocalDbService _localDbService;

        public GradeRepository(LocalDbService localDbService)
        {
            _localDbService = localDbService ?? throw new ArgumentNullException(nameof(localDbService));
        }

        public async Task<GradeModel> CreateAsync(GradeModel grade)
        {
            if (grade == null) throw new ArgumentNullException(nameof(grade));
            
            grade.CreatedAt = DateTime.Now;
            grade.UpdatedAt = DateTime.Now;
            grade.GradedDate = DateTime.Now;
            
            await _localDbService.GetConnection().InsertAsync(grade);
            Console.WriteLine($"Grade created in database: Student {grade.StudentId}, Assignment {grade.AssignmentId} (ID: {grade.GradeId})");
            return grade;
        }

        public async Task<GradeModel?> GetAsync(int id)
        {
            return await _localDbService.GetConnection().FindAsync<GradeModel>(id);
        }

        public async Task<IEnumerable<GradeModel>> GetAllAsync()
        {
            var grades = await _localDbService.GetConnection().Table<GradeModel>().ToListAsync();
            Console.WriteLine($"Retrieved {grades.Count} grades from database");
            return grades;
        }

        public async Task<GradeModel?> UpdateAsync(GradeModel grade)
        {
            if (grade == null) throw new ArgumentNullException(nameof(grade));

            var existingGrade = await _localDbService.GetConnection().FindAsync<GradeModel>(grade.GradeId);
            if (existingGrade == null) return null;

            // Update all properties
            existingGrade.StudentId = grade.StudentId;
            existingGrade.AssignmentId = grade.AssignmentId;
            existingGrade.TeacherId = grade.TeacherId;
            existingGrade.NumericalGrade = grade.NumericalGrade;
            existingGrade.MaxPoints = grade.MaxPoints;
            existingGrade.LetterGrade = grade.LetterGrade;
            existingGrade.Feedback = grade.Feedback;
            existingGrade.GradedDate = grade.GradedDate;
            existingGrade.UpdatedAt = DateTime.Now;

            await _localDbService.GetConnection().UpdateAsync(existingGrade);
            Console.WriteLine($"Grade updated in database: ID {grade.GradeId}");
            return existingGrade;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var grade = await _localDbService.GetConnection().FindAsync<GradeModel>(id);
            if (grade == null) return false;

            await _localDbService.GetConnection().DeleteAsync(grade);
            Console.WriteLine($"Grade deleted from database: ID {id}");
            return true;
        }

        public async Task<GradeModel?> GetGradeByStudentAndAssignmentAsync(int studentId, int assignmentId)
        {
            return await _localDbService.GetConnection()
                .Table<GradeModel>()
                .Where(g => g.StudentId == studentId && g.AssignmentId == assignmentId)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<GradeModel>> GetGradesByStudentAsync(int studentId)
        {
            Console.WriteLine($"GradeRepository: Getting grades for student {studentId}");
            var grades = await _localDbService.GetConnection()
                .Table<GradeModel>()
                .Where(g => g.StudentId == studentId)
                .OrderByDescending(g => g.GradedDate)
                .ToListAsync();

            Console.WriteLine($"GradeRepository: Found {grades.Count} grades for student {studentId}");
            foreach (var grade in grades)
            {
                Console.WriteLine($"  Grade: Assignment {grade.AssignmentId}, Score: {grade.NumericalGrade}/{grade.MaxPoints}");
            }

            return grades;
        }

        public async Task<IEnumerable<GradeModel>> GetGradesByAssignmentAsync(int assignmentId)
        {
            return await _localDbService.GetConnection()
                .Table<GradeModel>()
                .Where(g => g.AssignmentId == assignmentId)
                .OrderByDescending(g => g.GradedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<GradeModel>> GetGradesByTeacherAsync(int teacherId)
        {
            return await _localDbService.GetConnection()
                .Table<GradeModel>()
                .Where(g => g.TeacherId == teacherId)
                .OrderByDescending(g => g.GradedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<GradeModel>> GetGradesByClassAsync(string className)
        {
            // This requires joining with assignments to get class information
            // For now, we'll implement this in the service layer
            return await GetAllAsync();
        }

        public async Task<IEnumerable<GradeModel>> GetRecentGradesAsync(int count = 10)
        {
            return await _localDbService.GetConnection()
                .Table<GradeModel>()
                .OrderByDescending(g => g.GradedDate)
                .Take(count)
                .ToListAsync();
        }

        public async Task<decimal?> GetAverageGradeForStudentAsync(int studentId)
        {
            var grades = await GetGradesByStudentAsync(studentId);
            var numericalGrades = grades.Where(g => g.NumericalGrade.HasValue && g.MaxPoints > 0)
                                       .Select(g => (g.NumericalGrade.Value / g.MaxPoints) * 100);
            
            return numericalGrades.Any() ? (decimal?)numericalGrades.Average() : null;
        }

        public async Task<decimal?> GetAverageGradeForAssignmentAsync(int assignmentId)
        {
            var grades = await GetGradesByAssignmentAsync(assignmentId);
            var numericalGrades = grades.Where(g => g.NumericalGrade.HasValue && g.MaxPoints > 0)
                                       .Select(g => (g.NumericalGrade.Value / g.MaxPoints) * 100);
            
            return numericalGrades.Any() ? (decimal?)numericalGrades.Average() : null;
        }

        public async Task<int> GetGradedAssignmentsCountForStudentAsync(int studentId)
        {
            var grades = await GetGradesByStudentAsync(studentId);
            return grades.Count();
        }

        public async Task<int> GetTotalAssignmentsCountForStudentAsync(int studentId)
        {
            // This would require joining with assignments table
            // For now, return the graded count
            return await GetGradedAssignmentsCountForStudentAsync(studentId);
        }

        public async Task<bool> DeleteGradesByAssignmentAsync(int assignmentId)
        {
            var grades = await GetGradesByAssignmentAsync(assignmentId);
            foreach (var grade in grades)
            {
                await _localDbService.GetConnection().DeleteAsync(grade);
            }
            Console.WriteLine($"Deleted {grades.Count()} grades for assignment {assignmentId}");
            return true;
        }

        public async Task<bool> DeleteGradesByStudentAsync(int studentId)
        {
            var grades = await GetGradesByStudentAsync(studentId);
            foreach (var grade in grades)
            {
                await _localDbService.GetConnection().DeleteAsync(grade);
            }
            Console.WriteLine($"Deleted {grades.Count()} grades for student {studentId}");
            return true;
        }
    }
}
