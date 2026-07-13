using CareerConnect.Domain.Entities;
using Microsoft.EntityFrameworkCore;

// Alias to avoid conflict with the CareerConnect.Application namespace
using JobApplication = CareerConnect.Domain.Entities.Application;

namespace CareerConnect.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Job> Jobs => Set<Job>();
    public DbSet<JobApplication> Applications => Set<JobApplication>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure User -> UserProfile (1-to-1)
        modelBuilder.Entity<User>()
            .HasOne(u => u.UserProfile)
            .WithOne(up => up.User)
            .HasForeignKey<UserProfile>(up => up.UserId);

        // Configure UserProfile -> Application (1-to-many)
        modelBuilder.Entity<UserProfile>()
            .HasMany(up => up.Applications)
            .WithOne(a => a.UserProfile)
            .HasForeignKey(a => a.UserProfileId);

        // Configure Company -> Job (1-to-many)
        modelBuilder.Entity<Company>()
            .HasMany(c => c.Jobs)
            .WithOne(j => j.Company)
            .HasForeignKey(j => j.CompanyId);

        // Configure Job -> Application (1-to-many)
        modelBuilder.Entity<Job>()
            .HasMany(j => j.Applications)
            .WithOne(a => a.Job)
            .HasForeignKey(a => a.JobId);
            
        // Configure AdminUser for Company
        modelBuilder.Entity<Company>()
            .HasOne(c => c.AdminUser)
            .WithMany()
            .HasForeignKey(c => c.AdminUserId);
    }
}
