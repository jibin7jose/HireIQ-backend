using System.Security.Cryptography;
using CareerConnect.Domain.Entities;
using CareerConnect.Domain.Enums;
using CareerConnect.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CareerConnect.API.Seeding;

internal static class DevelopmentDataSeeder
{
    private static readonly Guid DemoEmployerId = Guid.Parse("e2c1c177-8593-42ca-bbd2-56ca4867bc12");
    private static readonly Guid DemoProfileId = Guid.Parse("10b8b02e-d015-45c9-8790-dd4b42e27f38");
    private static readonly Guid DemoCompanyId = Guid.Parse("6abf9aca-b31f-4dc9-8f2e-5375b5127733");
    private const string DemoEmployerEmail = "development-seed-owner@careerconnect.invalid";

    public static async Task SeedAsync(ApplicationDbContext db, CancellationToken cancellationToken = default)
    {
        var demoEmployer = await db.Users
            .Include(user => user.UserProfile)
            .SingleOrDefaultAsync(user => user.Id == DemoEmployerId, cancellationToken);

        if (demoEmployer is null)
        {
            demoEmployer = new User
            {
                Id = DemoEmployerId,
                Email = DemoEmployerEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(Convert.ToHexString(RandomNumberGenerator.GetBytes(32))),
                Role = UserRole.Employer,
                CreatedAt = DateTime.UtcNow,
                UserProfile = new UserProfile
                {
                    Id = DemoProfileId,
                    UserId = DemoEmployerId,
                    FullName = "CareerConnect Demo Company",
                    Phone = "+919876543210",
                    ReceiveJobAlerts = false
                }
            };

            db.Users.Add(demoEmployer);
        }
        else if (demoEmployer.UserProfile is null)
        {
            db.UserProfiles.Add(new UserProfile
            {
                Id = DemoProfileId,
                UserId = DemoEmployerId,
                FullName = "CareerConnect Demo Company",
                Phone = "+919876543210",
                ReceiveJobAlerts = false
            });
        }

        if (!await db.Companies.AnyAsync(company => company.Id == DemoCompanyId, cancellationToken))
        {
            db.Companies.Add(new Company
            {
                Id = DemoCompanyId,
                AdminUserId = DemoEmployerId,
                Name = "CareerConnect Demo Company",
                About = "A sample company used to populate the local development job board.",
                Location = "Bengaluru, India",
                LogoUrl = string.Empty,
                IsVerified = true
            });
        }

        var demoJobs = new[]
        {
            new Job
            {
                Id = Guid.Parse("90a1b31d-efeb-48de-8ecf-1fdba9cd3901"),
                CompanyId = DemoCompanyId,
                Title = "Junior Software Engineer",
                Description = "Build and maintain web features with a supportive engineering team. Work with C#, ASP.NET Core, React, and PostgreSQL while learning from experienced developers.",
                Location = "Bengaluru, India",
                JobType = "Full-Time",
                MinSalary = 600000,
                MaxSalary = 1000000,
                Latitude = 12.9716,
                Longitude = 77.5946,
                Status = JobStatus.Open,
                PostedAt = DateTime.UtcNow.AddDays(-1)
            },
            new Job
            {
                Id = Guid.Parse("90a1b31d-efeb-48de-8ecf-1fdba9cd3902"),
                CompanyId = DemoCompanyId,
                Title = "Product Designer",
                Description = "Design clear, accessible experiences for our web products. Partner with product managers and engineers to research user needs, prototype ideas, and improve shipped features.",
                Location = "Remote, India",
                JobType = "Full-Time",
                MinSalary = 800000,
                MaxSalary = 1400000,
                Status = JobStatus.Open,
                PostedAt = DateTime.UtcNow.AddDays(-2)
            },
            new Job
            {
                Id = Guid.Parse("90a1b31d-efeb-48de-8ecf-1fdba9cd3903"),
                CompanyId = DemoCompanyId,
                Title = "Data Analyst",
                Description = "Turn product and business data into useful insights. Build dashboards, answer questions with SQL, and share findings with teams across the company.",
                Location = "Mumbai, India",
                JobType = "Full-Time",
                MinSalary = 700000,
                MaxSalary = 1200000,
                Latitude = 19.0760,
                Longitude = 72.8777,
                Status = JobStatus.Open,
                PostedAt = DateTime.UtcNow.AddDays(-3)
            },
            new Job
            {
                Id = Guid.Parse("90a1b31d-efeb-48de-8ecf-1fdba9cd3904"),
                CompanyId = DemoCompanyId,
                Title = "Marketing Content Intern",
                Description = "Help create helpful content for candidates and employers. Support campaign planning, write social posts, and learn how a growing technology company reaches its users.",
                Location = "Bengaluru, India",
                JobType = "Part-Time",
                MinSalary = 180000,
                MaxSalary = 300000,
                Latitude = 12.9716,
                Longitude = 77.5946,
                Status = JobStatus.Open,
                PostedAt = DateTime.UtcNow.AddDays(-4)
            }
        };

        foreach (var job in demoJobs)
        {
            if (!await db.Jobs.AnyAsync(existingJob => existingJob.Id == job.Id, cancellationToken))
            {
                db.Jobs.Add(job);
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
