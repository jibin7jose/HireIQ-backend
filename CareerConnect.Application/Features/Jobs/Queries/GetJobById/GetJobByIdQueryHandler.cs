using CareerConnect.Application.DTOs.Jobs;
using CareerConnect.Application.Interfaces;
using CareerConnect.Domain.Entities;
using CareerConnect.Domain.Exceptions;
using MediatR;

namespace CareerConnect.Application.Features.Jobs.Queries.GetJobById;

public sealed class GetJobByIdQueryHandler : IRequestHandler<GetJobByIdQuery, JobDto>
{
    private readonly IJobRepository _jobRepository;

    public GetJobByIdQueryHandler(IJobRepository jobRepository)
    {
        _jobRepository = jobRepository;
    }

    public async Task<JobDto> Handle(GetJobByIdQuery request, CancellationToken cancellationToken)
    {
        var job = await _jobRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Job), request.Id);

        return new JobDto(
            Id:             job.Id,
            CompanyId:      job.CompanyId,
            CompanyName:    job.Company?.Name ?? string.Empty,
            CompanyLogoUrl: job.Company?.LogoUrl ?? string.Empty,
            Title:          job.Title,
            Description:    job.Description,
            Location:       job.Location,
            JobType:        job.JobType,
            MinSalary:      job.MinSalary,
            MaxSalary:      job.MaxSalary,
            Status:         job.Status.ToString(),
            PostedAt:       job.PostedAt
        );
    }
}
