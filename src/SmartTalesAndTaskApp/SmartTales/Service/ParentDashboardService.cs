using SmartTales.Data;
using SmartTales.Model;
using SmartTales.Repository.IRepository;

namespace SmartTales.Service
{
    public class ParentDashboardService
    {
        private readonly IUserRepository _userRepository;
        private readonly IParentChildRepository _parentChildRepository;
        private readonly IAssignmentRepository _assignmentRepository;
        private readonly IGradeRepository _gradeRepository;

        public ParentDashboardService(
            IUserRepository userRepository,
            IParentChildRepository parentChildRepository,
            IAssignmentRepository assignmentRepository,
            IGradeRepository gradeRepository)
        {
            _userRepository = userRepository;
            _parentChildRepository = parentChildRepository;
            _assignmentRepository = assignmentRepository;
            _gradeRepository = gradeRepository;
        }

        public async Task<List<MonthlyProgressData>> GetParentDashboardProgressAsync(int parentId)
        {
            try
            {
                // Get all children for this parent
                var children = await _userRepository.GetChildrenByParentIdAsync(parentId);
                var childIds = children.Select(c => c.Id).ToList();

                if (!childIds.Any())
                {
                    return GetEmptyProgressData();
                }

                // Get all assignments for the children
                var allAssignments = await _assignmentRepository.GetAllAsync();
                var childAssignments = allAssignments.Where(a => a.StudentId.HasValue && childIds.Contains(a.StudentId.Value));

                // Get all grades for the children
                var allGrades = new List<GradeModel>();
                foreach (var childId in childIds)
                {
                    var childGrades = await _gradeRepository.GetGradesByStudentAsync(childId);
                    allGrades.AddRange(childGrades);
                }

                // Calculate monthly progress for the last 6 months
                var progressData = new List<MonthlyProgressData>();
                var currentDate = DateTime.Now;

                for (int i = 5; i >= 0; i--)
                {
                    var targetMonth = currentDate.AddMonths(-i);
                    var monthData = await CalculateMonthlyProgress(childAssignments, allGrades, targetMonth);
                    progressData.Add(monthData);
                }

                return progressData;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calculating parent dashboard progress: {ex.Message}");
                return GetEmptyProgressData();
            }
        }

        private async Task<MonthlyProgressData> CalculateMonthlyProgress(
            IEnumerable<AssignmentModel> assignments, 
            List<GradeModel> grades, 
            DateTime targetMonth)
        {
            var monthStart = new DateTime(targetMonth.Year, targetMonth.Month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);

            // Get assignments for this month
            var monthAssignments = assignments.Where(a => 
                a.CreatedAt >= monthStart && a.CreatedAt <= monthEnd).ToList();

            // Get grades for this month
            var monthGrades = grades.Where(g => 
                g.GradedDate >= monthStart && g.GradedDate <= monthEnd).ToList();

            // Calculate stories read (assignments with type "ParentStory" that are submitted)
            var storiesRead = monthAssignments.Count(a => 
                a.AssignmentType == "ParentStory" && a.IsSubmitted);

            // Calculate tasks completed (all submitted assignments)
            var tasksCompleted = monthAssignments.Count(a => a.IsSubmitted);

            // Calculate total assignments for the month
            var totalAssignments = monthAssignments.Count();

            // Calculate progress percentage
            var progressPercentage = totalAssignments > 0 
                ? (int)Math.Round((double)tasksCompleted / totalAssignments * 100) 
                : 0;

            return new MonthlyProgressData
            {
                Month = targetMonth.ToString("MMMM"),
                Progress = progressPercentage,
                StoriesRead = storiesRead,
                TasksCompleted = tasksCompleted
            };
        }

        private List<MonthlyProgressData> GetEmptyProgressData()
        {
            var progressData = new List<MonthlyProgressData>();
            var currentDate = DateTime.Now;

            for (int i = 5; i >= 0; i--)
            {
                var targetMonth = currentDate.AddMonths(-i);
                progressData.Add(new MonthlyProgressData
                {
                    Month = targetMonth.ToString("MMMM"),
                    Progress = 0,
                    StoriesRead = 0,
                    TasksCompleted = 0
                });
            }

            return progressData;
        }

        public async Task<ParentDashboardSummary> GetParentDashboardSummaryAsync(int parentId)
        {
            try
            {
                // Get all children for this parent
                var children = await _userRepository.GetChildrenByParentIdAsync(parentId);
                var childIds = children.Select(c => c.Id).ToList();

                if (!childIds.Any())
                {
                    return new ParentDashboardSummary
                    {
                        TotalChildren = 0,
                        TotalStoriesSent = 0,
                        TotalAssignmentsCompleted = 0,
                        AverageProgress = 0
                    };
                }

                // Get all assignments for the children
                var allAssignments = await _assignmentRepository.GetAllAsync();
                var childAssignments = allAssignments.Where(a => a.StudentId.HasValue && childIds.Contains(a.StudentId.Value));

                // Calculate summary statistics
                var totalStoriesSent = childAssignments.Count(a => a.AssignmentType == "ParentStory");
                var totalAssignmentsCompleted = childAssignments.Count(a => a.IsSubmitted);
                var totalAssignments = childAssignments.Count();

                var averageProgress = totalAssignments > 0 
                    ? (int)Math.Round((double)totalAssignmentsCompleted / totalAssignments * 100) 
                    : 0;

                return new ParentDashboardSummary
                {
                    TotalChildren = children.Count(),
                    TotalStoriesSent = totalStoriesSent,
                    TotalAssignmentsCompleted = totalAssignmentsCompleted,
                    AverageProgress = averageProgress
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calculating parent dashboard summary: {ex.Message}");
                return new ParentDashboardSummary();
            }
        }
    }

    public class MonthlyProgressData
    {
        public string Month { get; set; } = string.Empty;
        public int Progress { get; set; }
        public int StoriesRead { get; set; }
        public int TasksCompleted { get; set; }
    }

    public class ParentDashboardSummary
    {
        public int TotalChildren { get; set; }
        public int TotalStoriesSent { get; set; }
        public int TotalAssignmentsCompleted { get; set; }
        public int AverageProgress { get; set; }
    }
}
