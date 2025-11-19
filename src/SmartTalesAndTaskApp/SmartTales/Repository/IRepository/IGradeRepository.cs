using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmartTales.Model;

namespace SmartTales.Repository.IRepository
{
    public interface IGradeRepository
    {
        // Basic CRUD operations
        Task<GradeModel> CreateAsync(GradeModel grade);
        Task<GradeModel?> GetAsync(int id);
        Task<IEnumerable<GradeModel>> GetAllAsync();
        Task<GradeModel?> UpdateAsync(GradeModel grade);
        Task<bool> DeleteAsync(int id);

        // Specific query methods
        Task<GradeModel?> GetGradeByStudentAndAssignmentAsync(int studentId, int assignmentId);
        Task<IEnumerable<GradeModel>> GetGradesByStudentAsync(int studentId);
        Task<IEnumerable<GradeModel>> GetGradesByAssignmentAsync(int assignmentId);
        Task<IEnumerable<GradeModel>> GetGradesByTeacherAsync(int teacherId);
        Task<IEnumerable<GradeModel>> GetGradesByClassAsync(string className);
        Task<IEnumerable<GradeModel>> GetRecentGradesAsync(int count = 10);

        // Statistical methods
        Task<decimal?> GetAverageGradeForStudentAsync(int studentId);
        Task<decimal?> GetAverageGradeForAssignmentAsync(int assignmentId);
        Task<int> GetGradedAssignmentsCountForStudentAsync(int studentId);
        Task<int> GetTotalAssignmentsCountForStudentAsync(int studentId);

        // Bulk operations
        Task<bool> DeleteGradesByAssignmentAsync(int assignmentId);
        Task<bool> DeleteGradesByStudentAsync(int studentId);
    }
}
