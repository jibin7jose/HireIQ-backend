using MediatR;

namespace CareerConnect.Application.Features.Users.Commands.DeleteAccount;

public record DeleteAccountCommand(Guid UserId) : IRequest;
