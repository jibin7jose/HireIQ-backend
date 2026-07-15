using CareerConnect.Application.DTOs.Jobs;
using MediatR;

namespace CareerConnect.Application.Features.Jobs.Queries.GetAllJobs;

public record GetAllJobsQuery(
    string? Location = null,
    string? JobType = null,
    string? Keyword = null
) : IRequest<IEnumerable<JobDto>>;
