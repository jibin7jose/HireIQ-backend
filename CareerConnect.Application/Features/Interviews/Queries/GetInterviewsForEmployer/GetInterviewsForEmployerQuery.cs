using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CareerConnect.Application.DTOs.Interviews;
using CareerConnect.Application.Interfaces;
using MediatR;

namespace CareerConnect.Application.Features.Interviews.Queries.GetInterviewsForEmployer;

public record GetInterviewsForEmployerQuery(Guid EmployerUserId) : IRequest<IEnumerable<InterviewDto>>;

public class GetInterviewsForEmployerQueryHandler : IRequestHandler<GetInterviewsForEmployerQuery, IEnumerable<InterviewDto>>
{
    private readonly IInterviewRepository _interviewRepository;

    public GetInterviewsForEmployerQueryHandler(IInterviewRepository interviewRepository)
    {
        _interviewRepository = interviewRepository;
    }

    public async Task<IEnumerable<InterviewDto>> Handle(GetInterviewsForEmployerQuery request, CancellationToken cancellationToken)
    {
        var interviews = await _interviewRepository.GetByEmployerIdAsync(request.EmployerUserId, cancellationToken);

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
