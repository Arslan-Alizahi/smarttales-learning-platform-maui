using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BCrypt.Net;
using SmartTales.Model;

namespace SmartTales.Data
{
    public class LocalDbService
    {
        private const string DB_NAME = "smarttales_local_db.db3";
        private readonly SQLiteAsyncConnection _connection;

        public LocalDbService()
        {
            var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), DB_NAME);
            Console.WriteLine($"Database path: {dbPath}");
            _connection = new SQLiteAsyncConnection(dbPath);

            // Create all tables
            _connection.CreateTableAsync<User>();
            _connection.CreateTableAsync<AssignmentModel>();
            _connection.CreateTableAsync<GradeModel>();
            _connection.CreateTableAsync<ParentChildModel>();
            _connection.CreateTableAsync<AdminUserModel>();
            _connection.CreateTableAsync<AdminAuditLogModel>();
            _connection.CreateTableAsync<PasswordResetRequestModel>();

            // Handle database migrations
            HandleDatabaseMigrationsAsync();

            InsertFirstUserIfNotExistsAsync();
            InsertSampleAssignmentsIfNotExistsAsync();
            InsertSampleParentChildRelationshipsIfNotExistsAsync();
            InsertDefaultAdminUserIfNotExistsAsync();
        }

        private async Task HandleDatabaseMigrationsAsync()
        {
            try
            {
                // Check if StudentId column exists in AssignmentModel table
                var tableInfo = await _connection.GetTableInfoAsync("Assignment");
                var hasStudentIdColumn = tableInfo.Any(column => column.Name == "StudentId");

                if (!hasStudentIdColumn)
                {
                    Console.WriteLine("Adding StudentId column to Assignment table...");
                    await _connection.ExecuteAsync("ALTER TABLE Assignment ADD COLUMN StudentId INTEGER");
                    Console.WriteLine("StudentId column added successfully");
                }

                // Check if Grade table exists, if not create it
                try
                {
                    var gradeTableInfo = await _connection.GetTableInfoAsync("Grade");
                    Console.WriteLine("Grade table already exists");
                }
                catch
                {
                    Console.WriteLine("Creating Grade table...");
                    await _connection.CreateTableAsync<GradeModel>();
                    Console.WriteLine("Grade table created successfully");
                }

                // Check if ParentChild table exists, if not create it
                try
                {
                    var parentChildTableInfo = await _connection.GetTableInfoAsync("ParentChild");
                    Console.WriteLine("ParentChild table already exists");
                }
                catch
                {
                    Console.WriteLine("Creating ParentChild table...");
                    await _connection.CreateTableAsync<ParentChildModel>();
                    Console.WriteLine("ParentChild table created successfully");
                }

                // Check if AdminUser table exists, if not create it
                try
                {
                    var adminUserTableInfo = await _connection.GetTableInfoAsync("AdminUser");
                    Console.WriteLine("AdminUser table already exists");
                }
                catch
                {
                    Console.WriteLine("Creating AdminUser table...");
                    await _connection.CreateTableAsync<AdminUserModel>();
                    Console.WriteLine("AdminUser table created successfully");
                }

                // Check if AdminAuditLog table exists, if not create it
                try
                {
                    var adminAuditLogTableInfo = await _connection.GetTableInfoAsync("AdminAuditLog");
                    Console.WriteLine("AdminAuditLog table already exists");
                }
                catch
                {
                    Console.WriteLine("Creating AdminAuditLog table...");
                    await _connection.CreateTableAsync<AdminAuditLogModel>();
                    Console.WriteLine("AdminAuditLog table created successfully");
                }

                // Check if PasswordResetRequest table exists, if not create it
                try
                {
                    var passwordResetTableInfo = await _connection.GetTableInfoAsync("PasswordResetRequest");
                    Console.WriteLine("PasswordResetRequest table already exists");
                }
                catch
                {
                    Console.WriteLine("Creating PasswordResetRequest table...");
                    await _connection.CreateTableAsync<PasswordResetRequestModel>();
                    Console.WriteLine("PasswordResetRequest table created successfully");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during database migration: {ex.Message}");
                // If migration fails, recreate the tables (this will lose data but ensures compatibility)
                try
                {
                    Console.WriteLine("Attempting to recreate tables...");
                    await _connection.DropTableAsync<AssignmentModel>();
                    await _connection.CreateTableAsync<AssignmentModel>();
                    await _connection.CreateTableAsync<GradeModel>();
                    Console.WriteLine("Tables recreated successfully");
                }
                catch (Exception recreateEx)
                {
                    Console.WriteLine($"Error recreating tables: {recreateEx.Message}");
                }
            }
        }

        private async Task InsertFirstUserIfNotExistsAsync()
        {
            try
            {
                var firstUser = await _connection.Table<User>().FirstOrDefaultAsync(u => u.Id == 1);
                if (firstUser == null)
                {
                    var hashedPassword = BCrypt.Net.BCrypt.HashPassword("Hussain");
                    await _connection.InsertAsync(new User
                    {
                        Id = 1,
                        FirstName = "Hussain",
                        LastName = "Ali",
                        Email = "hussain@gmail.com",
                        PhoneNumber = "03457265250",
                        Password = hashedPassword,
                        Role = "Admin"
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inserting first user: {ex.Message}");
            }
        }

        private async Task InsertSampleAssignmentsIfNotExistsAsync()
        {
            try
            {
                // Check if we already have assignments
                var existingAssignments = await _connection.Table<AssignmentModel>().CountAsync();
                if (existingAssignments == 0)
                {
                    // Add sample assignments for testing
                    var sampleAssignments = new List<AssignmentModel>
                    {
                        new AssignmentModel
                        {
                            Title = "Math Homework - Chapter 5",
                            Description = "Complete exercises 1-20 from Chapter 5. Show all your work and explain your reasoning for word problems.",
                            Class = "Grade 5",
                            DueDate = DateTime.Now.AddDays(3),
                            AssignmentType = "individual",
                            Points = 25,
                            TeacherName = "John Teacher",
                            IsPublished = true,
                            CreatedAt = DateTime.Now.AddDays(-1),
                            AttachmentsJson = "[]"
                        },
                        new AssignmentModel
                        {
                            Title = "Science Project - Solar System",
                            Description = "Create a model of the solar system using any materials you like. Include a written report about each planet.",
                            Class = "Grade 5",
                            DueDate = DateTime.Now.AddDays(7),
                            AssignmentType = "individual",
                            Points = 50,
                            TeacherName = "John Teacher",
                            IsPublished = true,
                            CreatedAt = DateTime.Now.AddDays(-2),
                            AttachmentsJson = "[]"
                        },
                        new AssignmentModel
                        {
                            Title = "Reading Assignment - Chapter 3",
                            Description = "Read Chapter 3 of 'The Adventures of Tom Sawyer' and answer the comprehension questions at the end.",
                            Class = "Grade 5",
                            DueDate = DateTime.Now.AddDays(2),
                            AssignmentType = "individual",
                            Points = 15,
                            TeacherName = "John Teacher",
                            IsPublished = true,
                            CreatedAt = DateTime.Now.AddDays(-3),
                            AttachmentsJson = "[]"
                        }
                    };

                    foreach (var assignment in sampleAssignments)
                    {
                        await _connection.InsertAsync(assignment);
                    }

                    Console.WriteLine($"Inserted {sampleAssignments.Count} sample assignments");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inserting sample assignments: {ex.Message}");
            }
        }

        private async Task InsertSampleParentChildRelationshipsIfNotExistsAsync()
        {
            try
            {
                // Check if we already have parent-child relationships
                var existingRelationships = await _connection.Table<ParentChildModel>().CountAsync();
                if (existingRelationships == 0)
                {
                    // First, let's create some sample users if they don't exist
                    var existingUsers = await _connection.Table<User>().CountAsync();
                    if (existingUsers <= 1) // Only admin user exists
                    {
                        // Create a sample parent
                        var sampleParent = new User
                        {
                            FirstName = "John",
                            LastName = "Parent",
                            Email = "john.parent@example.com",
                            PhoneNumber = "123-456-7890",
                            Password = BCrypt.Net.BCrypt.HashPassword("password123"),
                            Role = "Parent",
                            Address = "123 Main St, City, State"
                        };
                        await _connection.InsertAsync(sampleParent);

                        // Create sample children
                        var sampleChild1 = new User
                        {
                            FirstName = "Alice",
                            LastName = "Parent",
                            Email = "alice.child@example.com",
                            PhoneNumber = "123-456-7891",
                            Password = BCrypt.Net.BCrypt.HashPassword("password123"),
                            Role = "Kid",
                            GradeLevel = "Grade 3"
                        };
                        await _connection.InsertAsync(sampleChild1);

                        var sampleChild2 = new User
                        {
                            FirstName = "Bob",
                            LastName = "Parent",
                            Email = "bob.child@example.com",
                            PhoneNumber = "123-456-7892",
                            Password = BCrypt.Net.BCrypt.HashPassword("password123"),
                            Role = "Kid",
                            GradeLevel = "Grade 5"
                        };
                        await _connection.InsertAsync(sampleChild2);

                        // Create parent-child relationships
                        var parentChildRelationship1 = new ParentChildModel
                        {
                            ParentId = sampleParent.Id,
                            ChildId = sampleChild1.Id,
                            CreatedAt = DateTime.Now,
                            IsActive = true
                        };
                        await _connection.InsertAsync(parentChildRelationship1);

                        var parentChildRelationship2 = new ParentChildModel
                        {
                            ParentId = sampleParent.Id,
                            ChildId = sampleChild2.Id,
                            CreatedAt = DateTime.Now,
                            IsActive = true
                        };
                        await _connection.InsertAsync(parentChildRelationship2);

                        Console.WriteLine("Sample parent-child relationships created successfully");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inserting sample parent-child relationships: {ex.Message}");
            }
        }

        private async Task InsertDefaultAdminUserIfNotExistsAsync()
        {
            try
            {
                // Check if we already have admin users
                var existingAdmins = await _connection.Table<AdminUserModel>().CountAsync();
                if (existingAdmins == 0)
                {
                    // Create default super admin user
                    var defaultAdmin = new AdminUserModel
                    {
                        Username = "admin",
                        Email = "admin@smarttales.com",
                        FirstName = "System",
                        LastName = "Administrator",
                        Password = BCrypt.Net.BCrypt.HashPassword("admin123"), // Default password
                        Role = "SuperAdmin",
                        IsActive = true,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };

                    await _connection.InsertAsync(defaultAdmin);
                    Console.WriteLine("Default admin user created successfully");
                    Console.WriteLine("Username: admin, Password: admin123");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating default admin user: {ex.Message}");
            }
        }

        public SQLiteAsyncConnection GetConnection() => _connection;
    }
}
