using CareerConnect.Application.DTOs.Jobs;
using MediatR;

namespace CareerConnect.Application.Features.Jobs.Commands.CreateJob;

public record CreateJobCommand(
    Guid CompanyId,
    string Title,
    string Description,
    string Location,
    string JobType,
    decimal MinSalary,
    decimal MaxSalary
) : IRequest<JobDto>;
