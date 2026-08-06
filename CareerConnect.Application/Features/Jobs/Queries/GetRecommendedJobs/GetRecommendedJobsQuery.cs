using CareerConnect.Application.DTOs.Jobs;
using MediatR;

namespace CareerConnect.Application.Features.Jobs.Queries.GetRecommendedJobs;

public record GetRecommendedJobsQuery(Guid UserId) : IRequest<IEnumerable<JobDto>>;
