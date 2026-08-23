using CareerConnect.Application.Interfaces;
using CareerConnect.Domain.Entities;
using CareerConnect.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

// Alias to avoid conflict with CareerConnect.Application namespace
using JobApplication = CareerConnect.Domain.Entities.Application;

namespace CareerConnect.Infrastructure.Repositories;

public sealed class ApplicationRepository : IApplicationRepository
{
    private readonly ApplicationDbContext _context;

    public ApplicationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<JobApplication?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Applications
            .Include(a => a.Job)
                .ThenInclude(j => j!.Company)
            .Include(a => a.UserProfile)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public async Task<IEnumerable<JobApplication>> GetByJobIdAsync(Guid jobId, CancellationToken cancellationToken = default)
        => await _context.Applications
            .Include(a => a.UserProfile)
                .ThenInclude(up => up!.User)
            .Include(a => a.Interviews)
            .Where(a => a.JobId == jobId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<JobApplication>> GetByUserProfileIdAsync(Guid userProfileId, CancellationToken cancellationToken = default)
        => await _context.Applications
            .Include(a => a.Job)
                .ThenInclude(j => j!.Company)
            .Include(a => a.Interviews)
            .Where(a => a.UserProfileId == userProfileId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    public async Task<int> GetTotalApplicantsByCompanyIdAsync(Guid companyId, CancellationToken cancellationToken = default)
        => await _context.Applications
            .Include(a => a.Job)
            .Where(a => a.Job != null && a.Job.CompanyId == companyId)
            .CountAsync(cancellationToken);

    public async Task<int> GetAverageAiMatchScoreByCompanyIdAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        var scores = await _context.Applications
            .Include(a => a.Job)
            .Where(a => a.Job != null && a.Job.CompanyId == companyId && a.AiMatchScore > 0)
            .Select(a => a.AiMatchScore)
            .ToListAsync(cancellationToken);

        if (!scores.Any()) return 0;
        return (int)scores.Average();
    }

    public async Task<IEnumerable<JobApplication>> GetTopCandidatesByCompanyIdAsync(Guid companyId, int count, CancellationToken cancellationToken = default)
    {
        return await _context.Applications
            .Include(a => a.Job)
            .Include(a => a.UserProfile)
                .ThenInclude(up => up!.User)
            .Where(a => a.Job != null && a.Job.CompanyId == companyId && a.AiMatchScore > 0 && a.Status == CareerConnect.Domain.Enums.ApplicationStatus.Pending)
            .OrderByDescending(a => a.AiMatchScore)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid userProfileId, Guid jobId, CancellationToken cancellationToken = default)
        => await _context.Applications
            .AnyAsync(a => a.UserProfileId == userProfileId && a.JobId == jobId, cancellationToken);

    public async Task<IEnumerable<JobApplication>> GetByCandidateIdAsync(Guid userProfileId, CancellationToken cancellationToken = default)
    {
        return await _context.Applications
            .Include(a => a.Job)
            .ThenInclude(j => j!.Company)
            .Include(a => a.Interviews)
            .Where(a => a.UserProfileId == userProfileId)
            .OrderByDescending(a => a.AppliedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(JobApplication application, CancellationToken cancellationToken = default)
        => await _context.Applications.AddAsync(application, cancellationToken);

    public void Update(JobApplication application)
        => _context.Applications.Update(application);
}
