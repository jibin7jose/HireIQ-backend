using CareerConnect.Application.Interfaces;
using CareerConnect.Domain.Entities;
using CareerConnect.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CareerConnect.Infrastructure.Repositories;

public sealed class CompanyRepository : ICompanyRepository
{
    private readonly ApplicationDbContext _context;

    public CompanyRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Company?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Companies
            .Include(c => c.Jobs)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<Company?> GetByAdminUserIdAsync(Guid adminUserId, CancellationToken cancellationToken = default)
        => await _context.Companies
            .Include(c => c.Jobs)
            .FirstOrDefaultAsync(c => c.AdminUserId == adminUserId, cancellationToken);

    public async Task AddAsync(Company company, CancellationToken cancellationToken = default)
        => await _context.Companies.AddAsync(company, cancellationToken);

    public void Update(Company company)
        => _context.Companies.Update(company);
}
