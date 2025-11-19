using Microsoft.EntityFrameworkCore;
using Smart_Tales_and_Task_App.Models;
using SmartTales.Model;

namespace Smart_Tales_and_Task_App.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<AssignmentModel> Assignments { get; set; }
        public DbSet<UserModel> Users { get; set; }
    }
}
