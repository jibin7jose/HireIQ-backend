using CareerConnect.Application.DTOs.Companies;
using CareerConnect.Application.Interfaces;
using CareerConnect.Domain.Entities;
using CareerConnect.Domain.Enums;
using CareerConnect.Domain.Exceptions;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CareerConnect.Application.Features.Companies.Queries.GetEmployerStats;

public sealed class GetEmployerStatsQueryHandler : IRequestHandler<GetEmployerStatsQuery, EmployerStatsDto>
{
    private readonly ICompanyRepository _companyRepository;
    private readonly IApplicationRepository _applicationRepository;

    public GetEmployerStatsQueryHandler(ICompanyRepository companyRepository, IApplicationRepository applicationRepository)
    {
        _companyRepository = companyRepository;
        _applicationRepository = applicationRepository;
    }

    public async Task<EmployerStatsDto> Handle(GetEmployerStatsQuery request, CancellationToken cancellationToken)
    {
        var company = await _companyRepository.GetByAdminUserIdAsync(request.AdminUserId, cancellationToken)
            ?? throw new NotFoundException(nameof(Company), request.AdminUserId);

        var activeJobs = company.Jobs.Count(j => j.Status == JobStatus.Open);
        
        int totalApplicants = await _applicationRepository.GetTotalApplicantsByCompanyIdAsync(company.Id, cancellationToken);

        return new EmployerStatsDto(activeJobs, totalApplicants);
    }
}
