using CareerConnect.Application.Interfaces;
using CareerConnect.Infrastructure.Auth;
using CareerConnect.Infrastructure.Persistence;
using CareerConnect.Infrastructure.Repositories;
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

        return services;
    }
}
