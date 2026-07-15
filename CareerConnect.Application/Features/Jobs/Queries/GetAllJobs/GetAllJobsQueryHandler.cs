using CareerConnect.Application.DTOs.Jobs;
using CareerConnect.Application.Interfaces;
using MediatR;

namespace CareerConnect.Application.Features.Jobs.Queries.GetAllJobs;

public sealed class GetAllJobsQueryHandler : IRequestHandler<GetAllJobsQuery, IEnumerable<JobDto>>
{
    private readonly IJobRepository _jobRepository;

    public GetAllJobsQueryHandler(IJobRepository jobRepository)
    {
        _jobRepository = jobRepository;
    }

    public async Task<IEnumerable<JobDto>> Handle(GetAllJobsQuery request, CancellationToken cancellationToken)
    {
        var jobs = await _jobRepository.GetAllAsync(cancellationToken);

        // Client-side filtering (move to IJobRepository.GetFilteredAsync for production)
        if (!string.IsNullOrWhiteSpace(request.Keyword))
            jobs = jobs.Where(j =>
                j.Title.Contains(request.Keyword, StringComparison.OrdinalIgnoreCase) ||
                j.Description.Contains(request.Keyword, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(request.Location))
            jobs = jobs.Where(j => j.Location.Contains(request.Location, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(request.JobType))
            jobs = jobs.Where(j => j.JobType.Equals(request.JobType, StringComparison.OrdinalIgnoreCase));

        return jobs.Select(j => new JobDto(
            Id:             j.Id,
            CompanyId:      j.CompanyId,
            CompanyName:    j.Company?.Name ?? string.Empty,
            CompanyLogoUrl: j.Company?.LogoUrl ?? string.Empty,
            Title:          j.Title,
            Description:    j.Description,
            Location:       j.Location,
            JobType:        j.JobType,
            MinSalary:      j.MinSalary,
            MaxSalary:      j.MaxSalary,
            Status:         j.Status.ToString(),
            PostedAt:       j.PostedAt
        ));
    }
}
