using CareerConnect.Domain.Entities;

namespace CareerConnect.Application.Interfaces;

public interface ICompanyRepository
{
    Task<Company?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Company?> GetByAdminUserIdAsync(Guid adminUserId, CancellationToken cancellationToken = default);
    Task AddAsync(Company company, CancellationToken cancellationToken = default);
    void Update(Company company);
}
