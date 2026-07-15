using CareerConnect.Domain.Enums;

namespace CareerConnect.Application.DTOs.Auth;

public record RegisterRequest(
    string Email,
    string Password,
    string FullName,
    string Phone,
    UserRole Role
);

public record LoginRequest(
    string Email,
    string Password
);

public record AuthResponse(
    Guid UserId,
    string Email,
    string FullName,
    string Role,
    string Token
);
