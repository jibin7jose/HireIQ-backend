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
        
        int averageMatchScore = await _applicationRepository.GetAverageAiMatchScoreByCompanyIdAsync(company.Id, cancellationToken);
        
        var topCandidatesEntities = await _applicationRepository.GetTopCandidatesByCompanyIdAsync(company.Id, 3, cancellationToken);
        var topCandidates = topCandidatesEntities.Select(a => new TopCandidateDto(
            ApplicationId: a.Id,
            CandidateName: a.UserProfile?.FullName ?? "Unknown",
            JobTitle: a.Job?.Title ?? "Unknown",
            AiMatchScore: a.AiMatchScore
        )).ToList();

        return new EmployerStatsDto(activeJobs, totalApplicants, averageMatchScore, topCandidates);
    }
}
