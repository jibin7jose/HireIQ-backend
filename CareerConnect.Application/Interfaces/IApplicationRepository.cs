// Alias required: the entity 'Application' conflicts with the 'CareerConnect.Application' namespace
using JobApplication = CareerConnect.Domain.Entities.Application;

namespace CareerConnect.Application.Interfaces;

public interface IApplicationRepository
{
    Task<JobApplication?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<JobApplication>> GetByJobIdAsync(Guid jobId, CancellationToken cancellationToken = default);
    Task<IEnumerable<JobApplication>> GetByUserProfileIdAsync(Guid userProfileId, CancellationToken cancellationToken = default);
    Task<int> GetTotalApplicantsByCompanyIdAsync(Guid companyId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid userProfileId, Guid jobId, CancellationToken cancellationToken = default);
    Task<IEnumerable<JobApplication>> GetByCandidateIdAsync(Guid userProfileId, CancellationToken cancellationToken = default);
    Task AddAsync(JobApplication application, CancellationToken cancellationToken = default);
    void Update(JobApplication application);
}
