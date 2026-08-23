using MediatR;

namespace CareerConnect.Application.Features.Jobs.Commands.DeleteJob;

public record DeleteJobCommand(Guid Id, Guid RequestingUserId) : IRequest<Unit>;
