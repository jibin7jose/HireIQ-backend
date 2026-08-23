using CareerConnect.Application.DTOs.Jobs;
using CareerConnect.Application.Interfaces;
using CareerConnect.Domain.Entities;
using CareerConnect.Domain.Enums;
using CareerConnect.Domain.Exceptions;
using MediatR;
using System.Linq;

namespace CareerConnect.Application.Features.Jobs.Queries.GetDiscoverableJobs;

public class GetDiscoverableJobsQueryHandler : IRequestHandler<GetDiscoverableJobsQuery, IEnumerable<JobDto>>
{
    private readonly IJobRepository _jobRepository;
    private readonly IUserRepository _userRepository;
    private readonly IGeminiAiService _geminiAiService;
    private readonly IApplicationRepository _applicationRepository;

    public GetDiscoverableJobsQueryHandler(
        IJobRepository jobRepository,
        IUserRepository userRepository,
        IGeminiAiService geminiAiService,
        IApplicationRepository applicationRepository)
    {
        _jobRepository = jobRepository;
        _userRepository = userRepository;
        _geminiAiService = geminiAiService;
        _applicationRepository = applicationRepository;
    }

    public async Task<IEnumerable<JobDto>> Handle(GetDiscoverableJobsQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), request.UserId);

        var allJobs = await _jobRepository.GetAllAsync(cancellationToken);
        
        // Get all applications by this user
        var applications = await _applicationRepository.GetByCandidateIdAsync(request.UserId, cancellationToken);
        var appliedJobIds = applications.Select(a => a.JobId).ToHashSet();
        
        // Filter active jobs, taking 20 that the user has not applied to yet
        var activeJobs = allJobs
            .Where(j => j.Status == JobStatus.Open && !appliedJobIds.Contains(j.Id))
            .OrderByDescending(j => j.PostedAt)
            .Take(20)
            .ToList();

        if (activeJobs.Count == 0)
        {
            return Enumerable.Empty<JobDto>();
        }

        var profile = user.UserProfile;
        
        // If the user hasn't uploaded a resume/has no skills, just return without AI score
        if (profile == null || profile.Skills == null || !profile.Skills.Any())
        {
            return activeJobs.Select(j => MapToDto(j, null)).ToList();
        }

        var skillsStr = string.Join(", ", profile.Skills);
        var expStr = profile.ExperienceSummary ?? "No experience listed.";

        // Call Gemini for batch evaluation
        var scores = await _geminiAiService.BatchCalculateMatchScoresAsync(activeJobs, skillsStr, expStr, cancellationToken);

        // Map to DTO, inject the score, and sort by highest score first
        var discoverableJobs = activeJobs
            .Select(j => MapToDto(j, scores.TryGetValue(j.Id, out var s) ? s : 50))
            .OrderByDescending(j => j.AiMatchScore)
            .ToList();

        return discoverableJobs;
    }

    private static JobDto MapToDto(Job job, int? aiMatchScore)
    {
        return new JobDto(
            job.Id,
            job.CompanyId,
            job.Company?.Name ?? string.Empty,
            job.Company?.LogoUrl ?? string.Empty,
            job.Title,
            job.Description,
            job.Location,
            job.JobType,
            job.MinSalary,
            job.MaxSalary,
            job.Status.ToString(),
            job.PostedAt,
            job.Latitude,
            job.Longitude,
            aiMatchScore
        );
    }
}
