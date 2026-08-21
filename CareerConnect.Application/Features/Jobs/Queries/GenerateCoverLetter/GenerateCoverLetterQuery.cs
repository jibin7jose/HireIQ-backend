using MediatR;

namespace CareerConnect.Application.Features.Jobs.Queries.GenerateCoverLetter;

public record GenerateCoverLetterQuery(
    Guid JobId,
    Guid UserId
) : IRequest<string>;
