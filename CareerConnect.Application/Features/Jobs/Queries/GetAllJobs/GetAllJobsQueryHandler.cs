using CareerConnect.Application.DTOs;
using CareerConnect.Application.DTOs.Jobs;
using CareerConnect.Application.Interfaces;
using MediatR;

namespace CareerConnect.Application.Features.Jobs.Queries.GetAllJobs;

public sealed class GetAllJobsQueryHandler : IRequestHandler<GetAllJobsQuery, PagedResult<JobDto>>
{
    private readonly IJobRepository _jobRepository;

    public GetAllJobsQueryHandler(IJobRepository jobRepository)
    {
        _jobRepository = jobRepository;
    }

    public async Task<PagedResult<JobDto>> Handle(GetAllJobsQuery request, CancellationToken cancellationToken)
    {
        var (jobs, totalCount) = await _jobRepository.GetFilteredAsync(
            request.Keyword,
            request.Location,
            request.JobType,
            request.MinSalary,
            request.MaxSalary,
            request.PageNumber,
            request.PageSize,
            cancellationToken
        );

        var jobDtos = jobs.Select(j => new JobDto(
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

        return new PagedResult<JobDto>(
            Items: jobDtos,
            TotalCount: totalCount,
            PageNumber: request.PageNumber,
            PageSize: request.PageSize
        );
    }
}
