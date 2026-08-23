using CareerConnect.Application.DTOs.Auth;
using MediatR;

namespace CareerConnect.Application.Features.Auth.Commands.Login;

public record LoginCommand(
    string Email,
    string Password
) : IRequest<AuthResponse>;
