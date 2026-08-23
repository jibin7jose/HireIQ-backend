using MediatR;

namespace CareerConnect.Application.Features.Applications.Commands.UpdateApplicationStatus;

public record UpdateApplicationStatusCommand(
    Guid ApplicationId,
    Guid RequestingUserId,
    string Status) : IRequest<Unit>;


