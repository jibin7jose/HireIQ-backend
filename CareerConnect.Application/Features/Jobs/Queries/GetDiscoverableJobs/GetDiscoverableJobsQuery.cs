using CareerConnect.Application.DTOs.Jobs;
using MediatR;

namespace CareerConnect.Application.Features.Jobs.Queries.GetDiscoverableJobs;

public record GetDiscoverableJobsQuery(Guid UserId) : IRequest<IEnumerable<JobDto>>;
