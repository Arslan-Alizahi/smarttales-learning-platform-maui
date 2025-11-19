using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SmartTales.Data;
using SmartTales.Model;
using SmartTales.Repository.IRepository;

namespace SmartTales.Repository
{
    public class AssignmentRepository : IAssignmentRepository
    {
        private readonly LocalDbService _localDbService;

        public AssignmentRepository(LocalDbService localDbService)
        {
            _localDbService = localDbService ?? throw new ArgumentNullException(nameof(localDbService));
        }

        public async Task<AssignmentModel> CreateAsync(AssignmentModel assignment)
        {
            if (assignment == null) throw new ArgumentNullException(nameof(assignment));
            
            assignment.CreatedAt = DateTime.Now;
            await _localDbService.GetConnection().InsertAsync(assignment);
            Console.WriteLine($"Assignment created in database: {assignment.Title} (ID: {assignment.AssignmentId})");
            return assignment;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var assignment = await _localDbService.GetConnection().FindAsync<AssignmentModel>(id);
            if (assignment == null) return false;

            await _localDbService.GetConnection().DeleteAsync(assignment);
            Console.WriteLine($"Assignment deleted from database: ID {id}");
            return true;
        }

        public async Task<AssignmentModel?> GetAsync(int id)
        {
            return await _localDbService.GetConnection().FindAsync<AssignmentModel>(id);
        }

        public async Task<IEnumerable<AssignmentModel>> GetAllAsync()
        {
            var assignments = await _localDbService.GetConnection().Table<AssignmentModel>().ToListAsync();
            Console.WriteLine($"Retrieved {assignments.Count} assignments from database");
            return assignments;
        }

        public async Task<AssignmentModel?> UpdateAsync(AssignmentModel assignment)
        {
            if (assignment == null) throw new ArgumentNullException(nameof(assignment));

            var existingAssignment = await _localDbService.GetConnection().FindAsync<AssignmentModel>(assignment.AssignmentId);
            if (existingAssignment == null) return null;

            // Update all properties
            existingAssignment.Title = assignment.Title;
            existingAssignment.Description = assignment.Description;
            existingAssignment.Class = assignment.Class;
            existingAssignment.DueDate = assignment.DueDate;
            existingAssignment.AssignmentType = assignment.AssignmentType;
            existingAssignment.Points = assignment.Points;
            existingAssignment.TeacherName = assignment.TeacherName;
            existingAssignment.IsPublished = assignment.IsPublished;
            existingAssignment.AttachmentsJson = assignment.AttachmentsJson;
            existingAssignment.IsSubmitted = assignment.IsSubmitted;
            existingAssignment.SubmissionDate = assignment.SubmissionDate;
            existingAssignment.StudentId = assignment.StudentId;
            existingAssignment.SubmittedFilesJson = assignment.SubmittedFilesJson;
            existingAssignment.SubmissionNotes = assignment.SubmissionNotes;

            await _localDbService.GetConnection().UpdateAsync(existingAssignment);
            Console.WriteLine($"Assignment updated in database: {existingAssignment.Title} (ID: {existingAssignment.AssignmentId})");
            return existingAssignment;
        }

        public async Task<IEnumerable<AssignmentModel>> GetAssignmentsByClassAsync(string className)
        {
            return await _localDbService.GetConnection()
                .Table<AssignmentModel>()
                .Where(a => a.Class == className && a.IsPublished)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<AssignmentModel>> GetAssignmentsByTeacherAsync(string teacherName)
        {
            return await _localDbService.GetConnection()
                .Table<AssignmentModel>()
                .Where(a => a.TeacherName == teacherName)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<AssignmentModel>> GetPublishedAssignmentsAsync()
        {
            return await _localDbService.GetConnection()
                .Table<AssignmentModel>()
                .Where(a => a.IsPublished)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<AssignmentModel>> GetAssignmentsByTypeAsync(string assignmentType)
        {
            return await _localDbService.GetConnection()
                .Table<AssignmentModel>()
                .Where(a => a.AssignmentType == assignmentType)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<AssignmentModel>> GetSubmittedAssignmentsAsync()
        {
            return await _localDbService.GetConnection()
                .Table<AssignmentModel>()
                .Where(a => a.IsSubmitted && a.SubmissionDate != null)
                .OrderByDescending(a => a.SubmissionDate)
                .ToListAsync();
        }

        public async Task<bool> UpdateSubmissionAsync(int assignmentId, bool isSubmitted, DateTime? submissionDate, List<string> submittedFiles, string submissionNotes)
        {
            var assignment = await _localDbService.GetConnection().FindAsync<AssignmentModel>(assignmentId);
            if (assignment == null) return false;

            assignment.IsSubmitted = isSubmitted;
            assignment.SubmissionDate = submissionDate;
            assignment.SubmittedFiles = submittedFiles ?? new List<string>();
            assignment.SubmissionNotes = submissionNotes ?? string.Empty;

            await _localDbService.GetConnection().UpdateAsync(assignment);
            Console.WriteLine($"Assignment submission updated: ID {assignmentId}, Submitted: {isSubmitted}");
            return true;
        }
    }
}
