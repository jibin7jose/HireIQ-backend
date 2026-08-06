using CareerConnect.Domain.Enums;
using System;

namespace CareerConnect.Application.DTOs.Users;

public record UserDto(
    Guid Id,
    string Email,
    UserRole Role,
    DateTime CreatedAt
);
