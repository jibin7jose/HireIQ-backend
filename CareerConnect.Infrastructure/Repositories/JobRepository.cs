using CareerConnect.Application.Interfaces;
using CareerConnect.Domain.Entities;
using CareerConnect.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CareerConnect.Infrastructure.Repositories;

public sealed class JobRepository : IJobRepository
{
    private readonly ApplicationDbContext _context;

    public JobRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Job>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.Jobs
            .Include(j => j.Company)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    public async Task<Job?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Jobs
            .Include(j => j.Company)
            .Include(j => j.Applications)
            .FirstOrDefaultAsync(j => j.Id == id, cancellationToken);

    public async Task<IEnumerable<Job>> GetByCompanyIdAsync(Guid companyId, CancellationToken cancellationToken = default)
        => await _context.Jobs
            .Where(j => j.CompanyId == companyId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    public async Task<(IEnumerable<Job> Jobs, int TotalCount)> GetFilteredAsync(
        string? keyword,
        string? location,
        string? jobType,
        decimal? minSalary,
        decimal? maxSalary,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Jobs.Include(j => j.Company).AsNoTracking();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(j => EF.Functions.ILike(j.Title, $"%{keyword}%") || 
                                     EF.Functions.ILike(j.Description, $"%{keyword}%"));
        }

        if (!string.IsNullOrWhiteSpace(location))
        {
            query = query.Where(j => EF.Functions.ILike(j.Location, $"%{location}%"));
        }

        if (!string.IsNullOrWhiteSpace(jobType) && jobType != "All Types")
        {
            query = query.Where(j => j.JobType == jobType);
        }

        if (minSalary.HasValue)
        {
            query = query.Where(j => j.MaxSalary >= minSalary.Value); // Match if job max salary is at least user min
        }

        if (maxSalary.HasValue)
        {
            query = query.Where(j => j.MinSalary <= maxSalary.Value); // Match if job min salary is at most user max
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var jobs = await query
            .OrderByDescending(j => j.PostedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (jobs, totalCount);
    }

    public async Task AddAsync(Job job, CancellationToken cancellationToken = default)
        => await _context.Jobs.AddAsync(job, cancellationToken);

    public void Update(Job job)
        => _context.Jobs.Update(job);

    public void Delete(Job job)
        => _context.Jobs.Remove(job);
}
