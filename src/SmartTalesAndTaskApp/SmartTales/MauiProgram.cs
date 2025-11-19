using Microsoft.Extensions.Logging;
using SmartTales.Data;
using SmartTales.Repository;
using SmartTales.Repository.IRepository;
using SmartTales.Service;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using System.IO;

namespace SmartTales
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();
            
            // Add configuration from appsettings.json
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream("SmartTales.appsettings.json");
            
            if (stream != null)
            {
                builder.Configuration.AddJsonStream(stream);
            }
            
            // Register HttpClient as singleton
            builder.Services.AddSingleton<HttpClient>();
            
            // Register StoryService with HttpClient dependency
            builder.Services.AddScoped<StoryService>();

            // Register AssignmentService as scoped to work with database repositories
            builder.Services.AddScoped<AssignmentService>();

            // Register GradeService as scoped to work with database repositories
            builder.Services.AddScoped<GradeService>();

            // Register ParentDashboardService as scoped to work with database repositories
            builder.Services.AddScoped<ParentDashboardService>();

            // Register AdminService as scoped to work with database repositories
            builder.Services.AddScoped<AdminService>();
            
            // Register OCR and TTS services
            builder.Services.AddScoped<OCRService>();
            builder.Services.AddScoped<TTSService>();

            // Register Twilio SMS and Password Reset services
            builder.Services.AddScoped<TwilioSMSService>();
            builder.Services.AddScoped<PasswordResetService>();

            // Register file storage service
            builder.Services.AddSingleton<FileStorageService>();

            // Register database and repository services
            builder.Services.AddSingleton<LocalDbService>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IAssignmentRepository, AssignmentRepository>();
            builder.Services.AddScoped<IGradeRepository, GradeRepository>();
            builder.Services.AddScoped<IParentChildRepository, ParentChildRepository>();
            builder.Services.AddScoped<IAdminRepository, AdminRepository>();
            builder.Services.AddScoped<IPasswordResetRepository, PasswordResetRepository>();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
