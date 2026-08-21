using System;

namespace CareerConnect.Application.DTOs.Notifications;

public record NotificationDto(
    Guid Id,
    string Message,
    string Type,
    bool IsRead,
    DateTime Timestamp
);
