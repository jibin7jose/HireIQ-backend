using CareerConnect.Application.DTOs.Applications;
using MediatR;

namespace CareerConnect.Application.Features.Applications.Commands.ApplyToJob;

public record ApplyToJobCommand(
    Guid JobId,
    Guid UserId,
    string CoverLetter,
    string ResumeUrl
) : IRequest<ApplicationDto>;
