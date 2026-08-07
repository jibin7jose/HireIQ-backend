using System;
using System.Collections.Generic;
using MediatR;

namespace CareerConnect.Application.Features.Jobs.Queries.GetRecommendedCandidates;

public record GetRecommendedCandidatesQuery(Guid JobId, Guid EmployerId) : IRequest<IEnumerable<RecommendedCandidateDto>>;
