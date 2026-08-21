namespace CareerConnect.Application.DTOs.Companies;

public record EmployerStatsDto(
    int ActiveJobs,
    int TotalApplicants,
    int AverageMatchScore,
    System.Collections.Generic.List<TopCandidateDto> TopCandidates
);

public record TopCandidateDto(
    System.Guid ApplicationId,
    string CandidateName,
    string JobTitle,
    int AiMatchScore
);
