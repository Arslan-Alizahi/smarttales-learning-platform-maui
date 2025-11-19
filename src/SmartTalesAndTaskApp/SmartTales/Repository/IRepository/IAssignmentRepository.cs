using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmartTales.Model;

namespace SmartTales.Repository.IRepository
{
    public interface IAssignmentRepository
    {
        Task<AssignmentModel> CreateAsync(AssignmentModel assignment);
        Task<bool> DeleteAsync(int id);
        Task<AssignmentModel?> GetAsync(int id);
        Task<IEnumerable<AssignmentModel>> GetAllAsync();
        Task<AssignmentModel?> UpdateAsync(AssignmentModel assignment);
        Task<IEnumerable<AssignmentModel>> GetAssignmentsByClassAsync(string className);
        Task<IEnumerable<AssignmentModel>> GetAssignmentsByTeacherAsync(string teacherName);
        Task<IEnumerable<AssignmentModel>> GetPublishedAssignmentsAsync();
        Task<IEnumerable<AssignmentModel>> GetAssignmentsByTypeAsync(string assignmentType);
        Task<IEnumerable<AssignmentModel>> GetSubmittedAssignmentsAsync();
        Task<bool> UpdateSubmissionAsync(int assignmentId, bool isSubmitted, DateTime? submissionDate, List<string> submittedFiles, string submissionNotes);
    }
}
