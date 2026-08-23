using CareerConnect.Application.DTOs;
using CareerConnect.Application.DTOs.Jobs;
using MediatR;

namespace CareerConnect.Application.Features.Jobs.Queries.GetAllJobs;

public record GetAllJobsQuery(
    string? Location = null,
    string? JobType = null,
    string? Keyword = null,
    decimal? MinSalary = null,
    decimal? MaxSalary = null,
    int PageNumber = 1,
    int PageSize = 10
) : IRequest<PagedResult<JobDto>>;
