using CareerConnect.Application.DTOs.Auth;
using CareerConnect.Domain.Enums;
using MediatR;

namespace CareerConnect.Application.Features.Auth.Commands.Register;

public record RegisterCommand(
    string Email,
    string Password,
    string FullName,
    string Phone,
    UserRole Role
) : IRequest<AuthResponse>;
