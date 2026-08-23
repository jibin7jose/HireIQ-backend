namespace CareerConnect.Application.DTOs.Applications;

public record ApplicationDto(
    Guid Id,
    Guid JobId,
    string JobTitle,
    string CompanyName,
    string ApplicantName,
    string CoverLetter,
    string ResumeUrl,
    string Status,
    int AiMatchScore,
    DateTime AppliedAt,
    string? MeetingLink,
    DateTime? InterviewDate
);

public record ApplyRequest(
    Guid JobId,
    string? CoverLetter,
    string? ResumeUrl
);

public record UpdateApplicationStatusRequest(
    string Status
);
