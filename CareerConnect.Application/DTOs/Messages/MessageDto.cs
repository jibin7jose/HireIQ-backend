using System;

namespace CareerConnect.Application.DTOs.Messages;

public record MessageDto(
    Guid Id,
    Guid ApplicationId,
    Guid SenderUserId,
    string SenderName,
    Guid ReceiverUserId,
    string Content,
    DateTime SentAt,
    bool IsRead
);
