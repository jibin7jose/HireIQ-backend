using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CareerConnect.Application.Interfaces;
using MediatR;

namespace CareerConnect.Application.Features.Admin.Queries.GetDashboardMetrics;

public record GetDashboardMetricsQuery() : IRequest<DashboardMetricsDto>;

public class DashboardMetricsDto
{
    public int TotalUsers { get; set; }
    public int TotalJobs { get; set; }
    public int TotalCompanies { get; set; }
    public int TotalApplications { get; set; }
}

public class GetDashboardMetricsQueryHandler : IRequestHandler<GetDashboardMetricsQuery, DashboardMetricsDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IJobRepository _jobRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly IApplicationRepository _applicationRepository;

    public GetDashboardMetricsQueryHandler(
        IUserRepository userRepository,
        IJobRepository jobRepository,
        ICompanyRepository companyRepository,
        IApplicationRepository applicationRepository)
    {
        _userRepository = userRepository;
        _jobRepository = jobRepository;
        _companyRepository = companyRepository;
        _applicationRepository = applicationRepository;
    }

    public async Task<DashboardMetricsDto> Handle(GetDashboardMetricsQuery request, CancellationToken cancellationToken)
    {
        var users = await _userRepository.GetAllAsync(cancellationToken);
        var jobs = await _jobRepository.GetAllAsync(cancellationToken);
        var companies = await _companyRepository.GetAllAsync(cancellationToken);
        
        // Count applications by checking all jobs' applications count
        int totalApplications = jobs.Sum(j => j.Applications?.Count ?? 0);

        return new DashboardMetricsDto
        {
            TotalUsers = users.Count(),
            TotalJobs = jobs.Count(),
            TotalCompanies = companies.Count(),
            TotalApplications = totalApplications
        };
    }
}
