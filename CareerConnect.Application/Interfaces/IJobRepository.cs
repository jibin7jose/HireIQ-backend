using CareerConnect.Domain.Entities;
using CareerConnect.Domain.Enums;

namespace CareerConnect.Application.Interfaces;

public interface IJobRepository
{
    Task<IEnumerable<Job>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Job?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Job>> GetByCompanyIdAsync(Guid companyId, CancellationToken cancellationToken = default);
    Task AddAsync(Job job, CancellationToken cancellationToken = default);
    void Update(Job job);
    void Delete(Job job);
}
