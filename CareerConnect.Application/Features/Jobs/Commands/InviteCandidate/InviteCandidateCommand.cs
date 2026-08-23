using MediatR;
using System;

namespace CareerConnect.Application.Features.Jobs.Commands.InviteCandidate;

public record InviteCandidateCommand(
    Guid JobId,
    Guid CandidateUserId,
    Guid EmployerId,
    int AiMatchScore
) : IRequest<Unit>;
