using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CareerConnect.Application.DTOs.Interviews;
using CareerConnect.Application.Interfaces;
using MediatR;

namespace CareerConnect.Application.Features.Interviews.Queries.GetInterviewsForCandidate;

public record GetInterviewsForCandidateQuery(Guid CandidateUserId) : IRequest<IEnumerable<InterviewDto>>;

public class GetInterviewsForCandidateQueryHandler : IRequestHandler<GetInterviewsForCandidateQuery, IEnumerable<InterviewDto>>
{
    private readonly IInterviewRepository _interviewRepository;

    public GetInterviewsForCandidateQueryHandler(IInterviewRepository interviewRepository)
    {
        _interviewRepository = interviewRepository;
    }

    public async Task<IEnumerable<InterviewDto>> Handle(GetInterviewsForCandidateQuery request, CancellationToken cancellationToken)
    {
        var interviews = await _interviewRepository.GetByCandidateIdAsync(request.CandidateUserId, cancellationToken);

        return interviews.Select(i => new InterviewDto(
            Id: i.Id,
            ApplicationId: i.ApplicationId,
            JobTitle: i.Application?.Job?.Title ?? "",
            CompanyName: i.Application?.Job?.Company?.Name ?? "",
            CandidateName: i.Application?.UserProfile?.FullName ?? "",
            ScheduledAt: i.ScheduledAt,
            DurationMinutes: i.DurationMinutes,
            MeetingLink: i.MeetingLink,
            Status: i.Status.ToString()
        ));
    }
}
