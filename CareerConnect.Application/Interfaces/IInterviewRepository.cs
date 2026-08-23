using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CareerConnect.Domain.Entities;

namespace CareerConnect.Application.Interfaces;

public interface IInterviewRepository
{
    Task<Interview?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Interview>> GetByCandidateIdAsync(Guid candidateUserId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Interview>> GetByEmployerIdAsync(Guid employerUserId, CancellationToken cancellationToken = default);
    Task AddAsync(Interview interview, CancellationToken cancellationToken = default);
    void Update(Interview interview);
}
