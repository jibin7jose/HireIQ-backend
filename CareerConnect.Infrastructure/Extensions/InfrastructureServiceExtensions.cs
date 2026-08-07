using CareerConnect.Application.Interfaces;
using CareerConnect.Infrastructure.Auth;
using CareerConnect.Infrastructure.Persistence;
using CareerConnect.Infrastructure.Repositories;
using CareerConnect.Infrastructure.Services;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CareerConnect.Infrastructure.Extensions;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // EF Core + PostgreSQL
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        // Repositories
        services.AddScoped<IUserRepository,        UserRepository>();
        services.AddScoped<IJobRepository,         JobRepository>();
        services.AddScoped<ICompanyRepository,     CompanyRepository>();
        services.AddScoped<IApplicationRepository, ApplicationRepository>();
        services.AddScoped<IInterviewRepository,   InterviewRepository>();
        services.AddScoped<IMessageRepository,     MessageRepository>();
        services.AddScoped<IUnitOfWork,            UnitOfWork>();

        // Services
        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddScoped<IAiScoringService, AiScoringService>();
        services.AddScoped<IJobScheduler, HangfireJobScheduler>();
        services.AddScoped<IStorageService, SupabaseStorageService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<INotificationService, SignalRNotificationService>();

        services.AddHttpClient<IGeminiAiService, GeminiAiService>();

        // Supabase Client
        services.AddSingleton(provider =>
        {
            var url = configuration["Supabase:Url"] ?? throw new ArgumentNullException("Supabase:Url is missing");
            var key = configuration["Supabase:Key"] ?? throw new ArgumentNullException("Supabase:Key is missing");
            var options = new Supabase.SupabaseOptions { AutoConnectRealtime = false };
            return new Supabase.Client(url, key, options);
        });

        // Hangfire Background Jobs
        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UsePostgreSqlStorage(c => c.UseNpgsqlConnection(configuration.GetConnectionString("DefaultConnection"))));

        services.AddHangfireServer();

        return services;
    }
}
