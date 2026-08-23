using System;
using System.Collections.Generic;
using CareerConnect.Domain.Enums;

namespace CareerConnect.Application.Features.Jobs.Queries.GetRecommendedCandidates;

public record RecommendedCandidateDto(
    Guid CandidateId,
    string FullName,
    string AvatarUrl,
    string ExperienceSummary,
    List<string> Skills,
    int AiMatchScore
);
