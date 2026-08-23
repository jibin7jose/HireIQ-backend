using System;

namespace CareerConnect.Application.DTOs.Interviews;

public record InterviewDto(
    Guid Id,
    Guid ApplicationId,
    string JobTitle,
    string CompanyName,
    string CandidateName,
    DateTime ScheduledAt,
    int DurationMinutes,
    string MeetingLink,
    string Status
);
