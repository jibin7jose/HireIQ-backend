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

    public async Task AddAsync(Job job, CancellationToken cancellationToken = default)
        => await _context.Jobs.AddAsync(job, cancellationToken);

    public void Update(Job job)
        => _context.Jobs.Update(job);

    public void Delete(Job job)
        => _context.Jobs.Remove(job);
}
