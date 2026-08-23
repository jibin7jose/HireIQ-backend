using CareerConnect.Application.DTOs;
using CareerConnect.Application.DTOs.Jobs;
using MediatR;

namespace CareerConnect.Application.Features.Jobs.Queries.SemanticJobSearch;

public record SemanticJobSearchQuery(
    string SemanticQuery,
    int PageNumber = 1,
    int PageSize = 10
) : IRequest<PagedResult<JobDto>>;
