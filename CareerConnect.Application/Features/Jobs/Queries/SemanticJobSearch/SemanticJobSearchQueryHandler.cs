using CareerConnect.Application.DTOs;
using CareerConnect.Application.DTOs.Jobs;
using CareerConnect.Application.Interfaces;
using MediatR;

namespace CareerConnect.Application.Features.Jobs.Queries.SemanticJobSearch;

public sealed class SemanticJobSearchQueryHandler : IRequestHandler<SemanticJobSearchQuery, PagedResult<JobDto>>
{
    private readonly IGeminiAiService _geminiAiService;
    private readonly IJobRepository _jobRepository;

    public SemanticJobSearchQueryHandler(IGeminiAiService geminiAiService, IJobRepository jobRepository)
    {
        _geminiAiService = geminiAiService;
        _jobRepository = jobRepository;
    }

    public async Task<PagedResult<JobDto>> Handle(SemanticJobSearchQuery request, CancellationToken cancellationToken)
    {
        // 1. Ask Gemini to extract parameters
        var semanticParams = await _geminiAiService.ExtractSearchParametersAsync(request.SemanticQuery, cancellationToken);

        // 2. Query Job Repository with extracted parameters
        var (jobs, totalCount) = await _jobRepository.GetFilteredAsync(
            semanticParams.Keyword,
            semanticParams.Location,
            semanticParams.JobType,
            semanticParams.MinSalary,
            null, // maxSalary
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
            PostedAt:       j.PostedAt,
            Latitude:       j.Latitude,
            Longitude:      j.Longitude
        ));

        return new PagedResult<JobDto>(
            Items: jobDtos,
            TotalCount: totalCount,
            PageNumber: request.PageNumber,
            PageSize: request.PageSize
        );
    }
}
