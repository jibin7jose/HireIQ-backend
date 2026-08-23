using CareerConnect.Domain.Enums;
using MediatR;

namespace CareerConnect.Application.Features.Jobs.Commands.UpdateJob;

public record UpdateJobCommand(
    Guid Id,
    Guid RequestingUserId,
    string? Title,
    string? Description,
    string? Location,
    string? JobType,
    decimal? MinSalary,
    decimal? MaxSalary,
    JobStatus? Status
) : IRequest<Unit>;
