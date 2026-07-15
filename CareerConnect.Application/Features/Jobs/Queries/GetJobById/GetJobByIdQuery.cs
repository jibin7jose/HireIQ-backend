using CareerConnect.Application.DTOs.Jobs;
using MediatR;

namespace CareerConnect.Application.Features.Jobs.Queries.GetJobById;

public record GetJobByIdQuery(Guid Id) : IRequest<JobDto>;
