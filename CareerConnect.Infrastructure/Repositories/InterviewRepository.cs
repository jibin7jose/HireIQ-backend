using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CareerConnect.Application.Interfaces;
using CareerConnect.Domain.Entities;
using CareerConnect.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CareerConnect.Infrastructure.Repositories;

public class InterviewRepository : IInterviewRepository
{
    private readonly ApplicationDbContext _context;

    public InterviewRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Interview?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Interviews
            .Include(i => i.Application)
                .ThenInclude(a => a.UserProfile)
            .Include(i => i.Application)
                .ThenInclude(a => a.Job)
                    .ThenInclude(j => j.Company)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Interview>> GetByCandidateIdAsync(Guid candidateUserId, CancellationToken cancellationToken = default)
    {
        return await _context.Interviews
            .Include(i => i.Application)
                .ThenInclude(a => a.UserProfile)
            .Include(i => i.Application)
                .ThenInclude(a => a.Job)
                    .ThenInclude(j => j.Company)
            .Where(i => i.Application != null && i.Application.UserProfile != null && i.Application.UserProfile.UserId == candidateUserId)
            .OrderBy(i => i.ScheduledAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Interview>> GetByEmployerIdAsync(Guid employerUserId, CancellationToken cancellationToken = default)
    {
        return await _context.Interviews
            .Include(i => i.Application)
                .ThenInclude(a => a.UserProfile)
            .Include(i => i.Application)
                .ThenInclude(a => a.Job)
                    .ThenInclude(j => j.Company)
            .Where(i => i.Application != null && i.Application.Job != null && i.Application.Job.Company != null && i.Application.Job.Company.AdminUserId == employerUserId)
            .OrderBy(i => i.ScheduledAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Interview interview, CancellationToken cancellationToken = default)
    {
        await _context.Interviews.AddAsync(interview, cancellationToken);
    }

    public void Update(Interview interview)
    {
        _context.Interviews.Update(interview);
    }
}
