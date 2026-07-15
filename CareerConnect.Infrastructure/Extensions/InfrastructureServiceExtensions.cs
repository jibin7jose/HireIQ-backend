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
        services.AddScoped<IUnitOfWork,            UnitOfWork>();

        // Services
        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddScoped<IAiScoringService, AiScoringService>();
        services.AddScoped<IJobScheduler, HangfireJobScheduler>();

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
