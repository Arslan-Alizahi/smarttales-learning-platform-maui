using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SmartTales.Model;
using SmartTales.Repository.IRepository;
using SmartTales.Data;

namespace SmartTales.Service
{
    public class AssignmentService
    {
        private readonly IAssignmentRepository _assignmentRepository;
        private readonly IUserRepository _userRepository;

        // Event that components can subscribe to for notifications when assignments change
        public event Action OnAssignmentsChanged;

        public AssignmentService(IAssignmentRepository assignmentRepository, IUserRepository userRepository)
        {
            _assignmentRepository = assignmentRepository ?? throw new ArgumentNullException(nameof(assignmentRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        // Method to notify subscribers that assignments have changed
        private void NotifyAssignmentsChanged()
        {
            OnAssignmentsChanged?.Invoke();
        }

        // Add a new assignment
        public async Task<AssignmentModel> AddAssignment(AssignmentModel assignment)
        {
            var createdAssignment = await _assignmentRepository.CreateAsync(assignment);

            // Notify subscribers that assignments have changed
            NotifyAssignmentsChanged();

            return createdAssignment;
        }

        // Get all assignments
        public async Task<List<AssignmentModel>> GetAllAssignments()
        {
            var assignments = await _assignmentRepository.GetAllAsync();
            return assignments.ToList();
        }

        // Get assignments for a specific class
        public async Task<List<AssignmentModel>> GetAssignmentsByClass(string className)
        {
            var assignments = await _assignmentRepository.GetAssignmentsByClassAsync(className);
            return assignments.ToList();
        }

        // Get assignments by teacher name
        public async Task<List<AssignmentModel>> GetAssignmentsByTeacher(string teacherName)
        {
            var assignments = await _assignmentRepository.GetAssignmentsByTeacherAsync(teacherName);
            return assignments.ToList();
        }

        // Get a specific assignment by ID
        public async Task<AssignmentModel> GetAssignmentById(int id)
        {
            return await _assignmentRepository.GetAsync(id);
        }

        // Update an existing assignment
        public async Task<bool> UpdateAssignment(AssignmentModel assignment)
        {
            var updatedAssignment = await _assignmentRepository.UpdateAsync(assignment);
            if (updatedAssignment == null)
                return false;

            // Notify subscribers that assignments have changed
            NotifyAssignmentsChanged();

            return true;
        }

        // Delete an assignment
        public async Task<bool> DeleteAssignment(int id)
        {
            var result = await _assignmentRepository.DeleteAsync(id);

            if (result)
            {
                // Notify subscribers that assignments have changed
                NotifyAssignmentsChanged();
            }

            return result;
        }

        // Get published assignments only
        public async Task<List<AssignmentModel>> GetPublishedAssignments()
        {
            var assignments = await _assignmentRepository.GetPublishedAssignmentsAsync();
            return assignments.ToList();
        }

        // Get submitted assignments only
        public async Task<List<AssignmentModel>> GetSubmittedAssignments()
        {
            var assignments = await _assignmentRepository.GetSubmittedAssignmentsAsync();
            return assignments.ToList();
        }

        // Get submitted assignments with student information
        public async Task<List<SubmissionViewModel>> GetSubmissionsWithStudentInfo()
        {
            var submittedAssignments = await _assignmentRepository.GetSubmittedAssignmentsAsync();
            var allUsers = await _userRepository.GetAllAsync();
            var students = allUsers.Where(u => u.Role == "Kid").ToDictionary(u => u.Id, u => u);

            var submissions = new List<SubmissionViewModel>();

            foreach (var assignment in submittedAssignments)
            {
                var submission = new SubmissionViewModel
                {
                    AssignmentId = assignment.AssignmentId,
                    AssignmentTitle = assignment.Title,
                    AssignmentDescription = assignment.Description,
                    Class = assignment.Class,
                    DueDate = assignment.DueDate,
                    Points = assignment.Points,
                    TeacherName = assignment.TeacherName,
                    SubmissionDate = assignment.SubmissionDate,
                    SubmittedFiles = assignment.SubmittedFiles,
                    SubmissionNotes = assignment.SubmissionNotes
                };

                // Try to find student information
                if (assignment.StudentId.HasValue && students.ContainsKey(assignment.StudentId.Value))
                {
                    var student = students[assignment.StudentId.Value];
                    submission.StudentId = student.Id;
                    submission.StudentName = $"{student.FirstName} {student.LastName}";
                    submission.StudentInitials = $"{student.FirstName.FirstOrDefault()}{student.LastName.FirstOrDefault()}";
                    submission.StudentEmail = student.Email;
                    submission.GradeLevel = student.GradeLevel ?? "";
                }
                else
                {
                    // Fallback for assignments without StudentId (legacy data)
                    submission.StudentName = "Unknown Student";
                    submission.StudentInitials = "??";
                }

                submissions.Add(submission);
            }

            return submissions.OrderByDescending(s => s.SubmissionDate).ToList();
        }

        // Update assignment submission
        public async Task<bool> UpdateAssignmentSubmission(int assignmentId, bool isSubmitted, DateTime? submissionDate, List<string> submittedFiles, string submissionNotes)
        {
            var result = await _assignmentRepository.UpdateSubmissionAsync(assignmentId, isSubmitted, submissionDate, submittedFiles, submissionNotes);

            if (result)
            {
                // Notify subscribers that assignments have changed
                NotifyAssignmentsChanged();
            }

            return result;
        }
    }
} 