using CareerConnect.Application.DTOs.Jobs;
using CareerConnect.Application.Interfaces;
using CareerConnect.Domain.Entities;
using CareerConnect.Domain.Enums;
using CareerConnect.Domain.Exceptions;
using MediatR;
using System.Linq;

namespace CareerConnect.Application.Features.Jobs.Queries.GetRecommendedJobs;

public class GetRecommendedJobsQueryHandler : IRequestHandler<GetRecommendedJobsQuery, IEnumerable<JobDto>>
{
    private readonly IJobRepository _jobRepository;
    private readonly IUserRepository _userRepository;
    private readonly IGeminiAiService _geminiAiService;

    public GetRecommendedJobsQueryHandler(
        IJobRepository jobRepository,
        IUserRepository userRepository,
        IGeminiAiService geminiAiService)
    {
        _jobRepository = jobRepository;
        _userRepository = userRepository;
        _geminiAiService = geminiAiService;
    }

    public async Task<IEnumerable<JobDto>> Handle(GetRecommendedJobsQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), request.UserId);

        var allJobs = await _jobRepository.GetAllAsync(cancellationToken);
        
        // Filter active jobs, taking the 20 most recent to avoid overwhelming AI or hitting timeouts
        var activeJobs = allJobs
            .Where(j => j.Status == JobStatus.Open)
            .OrderByDescending(j => j.PostedAt)
            .Take(20)
            .ToList();

        if (activeJobs.Count == 0)
        {
            return Enumerable.Empty<JobDto>();
        }

        var profile = user.UserProfile;
        
        // If the user hasn't uploaded a resume/has no skills, just return the recent jobs without AI score
        if (profile == null || profile.Skills == null || !profile.Skills.Any())
        {
            return activeJobs.Select(j => MapToDto(j, null)).ToList();
        }

        var skillsStr = string.Join(", ", profile.Skills);
        var expStr = profile.ExperienceSummary ?? "No experience listed.";

        // Call Gemini for batch evaluation
        var scores = await _geminiAiService.BatchCalculateMatchScoresAsync(activeJobs, skillsStr, expStr, cancellationToken);

        // Map to DTO, inject the score, and sort by highest score first
        var recommendedJobs = activeJobs
            .Select(j => MapToDto(j, scores.TryGetValue(j.Id, out var s) ? s : 50))
            .OrderByDescending(j => j.AiMatchScore)
            .Take(5) // Only return the top 5 best matches to the frontend dashboard
            .ToList();

        return recommendedJobs;
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
            aiMatchScore
        );
    }
}
