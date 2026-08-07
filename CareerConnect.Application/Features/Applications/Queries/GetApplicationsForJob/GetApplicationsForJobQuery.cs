using CareerConnect.Application.DTOs.Applications;
using CareerConnect.Application.Interfaces;
using CareerConnect.Domain.Entities;
using CareerConnect.Domain.Exceptions;
using MediatR;

using CareerConnect.Domain.Enums;

namespace CareerConnect.Application.Features.Applications.Queries.GetApplicationsForJob;

public record GetApplicationsForJobQuery(Guid JobId, Guid RequestingCompanyId, ApplicationStatus? StatusFilter = null) : IRequest<List<EmployerApplicationDto>>;

public class GetApplicationsForJobQueryHandler : IRequestHandler<GetApplicationsForJobQuery, List<EmployerApplicationDto>>
{
    private readonly IApplicationRepository _applicationRepository;
    private readonly IJobRepository _jobRepository;

    public GetApplicationsForJobQueryHandler(
        IApplicationRepository applicationRepository,
        IJobRepository jobRepository)
    {
        _applicationRepository = applicationRepository;
        _jobRepository = jobRepository;
    }

    public async Task<List<EmployerApplicationDto>> Handle(GetApplicationsForJobQuery request, CancellationToken cancellationToken)
    {
        var job = await _jobRepository.GetByIdAsync(request.JobId, cancellationToken)
            ?? throw new NotFoundException(nameof(Job), request.JobId);

        if (job.CompanyId != request.RequestingCompanyId)
        {
            throw new UnauthorizedAccessException("You can only view applications for jobs posted by your company.");
        }

        var applications = await _applicationRepository.GetByJobIdAsync(request.JobId, cancellationToken);
        
        if (request.StatusFilter.HasValue)
        {
            applications = applications.Where(a => a.Status == request.StatusFilter.Value).ToList();
        }

        return applications.Select(a => new EmployerApplicationDto
        {
            Id = a.Id,
            JobId = a.JobId,
            UserProfileId = a.UserProfileId,
            CandidateName = a.UserProfile?.FullName ?? "Unknown",
            CandidateEmail = a.UserProfile?.User?.Email ?? "Unknown",
            CandidatePhone = a.UserProfile?.Phone ?? "Unknown",
            CandidateSkills = a.UserProfile?.Skills ?? new List<string>(),
            CoverLetter = a.CoverLetter,
            ResumeUrl = a.ResumeUrl,
            Status = a.Status,
            AiMatchScore = a.AiMatchScore,
            AppliedAt = a.AppliedAt,
            MeetingLink = a.Interviews.OrderByDescending(i => i.ScheduledAt).FirstOrDefault()?.MeetingLink,
            InterviewDate = a.Interviews.OrderByDescending(i => i.ScheduledAt).FirstOrDefault()?.ScheduledAt
        }).OrderByDescending(a => a.AiMatchScore).ToList();
    }
}
