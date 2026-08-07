using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CareerConnect.Application.Interfaces;
using CareerConnect.Domain.Enums;
using CareerConnect.Domain.Exceptions;
using MediatR;

namespace CareerConnect.Application.Features.Jobs.Queries.GetRecommendedCandidates;

public class GetRecommendedCandidatesQueryHandler : IRequestHandler<GetRecommendedCandidatesQuery, IEnumerable<RecommendedCandidateDto>>
{
    private readonly IJobRepository _jobRepository;
    private readonly IUserRepository _userRepository;
    private readonly IApplicationRepository _applicationRepository;
    private readonly IGeminiAiService _geminiAiService;

    public GetRecommendedCandidatesQueryHandler(
        IJobRepository jobRepository,
        IUserRepository userRepository,
        IApplicationRepository applicationRepository,
        IGeminiAiService geminiAiService)
    {
        _jobRepository = jobRepository;
        _userRepository = userRepository;
        _applicationRepository = applicationRepository;
        _geminiAiService = geminiAiService;
    }

    public async Task<IEnumerable<RecommendedCandidateDto>> Handle(GetRecommendedCandidatesQuery request, CancellationToken cancellationToken)
    {
        var job = await _jobRepository.GetByIdAsync(request.JobId, cancellationToken)
            ?? throw new NotFoundException("Job", request.JobId);

        if (job.Company?.AdminUserId != request.EmployerId)
        {
            throw new UnauthorizedAccessException("You don't have permission to view recommendations for this job.");
        }

        // Get candidates who opted in
        var users = await _userRepository.GetAllAsync(cancellationToken);
        var candidates = users.Where(u => 
            u.Role == UserRole.Candidate && 
            u.UserProfile != null && 
            u.UserProfile.Skills.Any() &&
            u.UserProfile.ReceiveJobAlerts).ToList();

        // Get candidates who ALREADY applied
        var appliedUserIds = new HashSet<Guid>();
        var existingApplications = await _applicationRepository.GetByJobIdAsync(request.JobId, cancellationToken);
        foreach (var app in existingApplications)
        {
            appliedUserIds.Add(app.UserProfileId);
        }

        // Filter out candidates who already applied
        var eligibleCandidates = candidates.Where(c => !appliedUserIds.Contains(c.UserProfile!.Id)).ToList();

        var recommendations = new List<RecommendedCandidateDto>();

        foreach (var candidate in eligibleCandidates)
        {
            var profile = candidate.UserProfile!;
            var candidateSkillsStr = string.Join(", ", profile.Skills);

            try
            {
                var score = await _geminiAiService.CalculateMatchScoreAsync(
                    job.Description,
                    candidateSkillsStr,
                    profile.ExperienceSummary,
                    cancellationToken);

                if (score >= 60) // Return matches with score 60 or above
                {
                    recommendations.Add(new RecommendedCandidateDto(
                        CandidateId: candidate.Id,
                        FullName: profile.FullName,
                        AvatarUrl: profile.AvatarUrl,
                        ExperienceSummary: profile.ExperienceSummary,
                        Skills: profile.Skills,
                        AiMatchScore: score
                    ));
                }
            }
            catch
            {
                // Ignore scoring failures for individual candidates
            }
        }

        return recommendations.OrderByDescending(x => x.AiMatchScore);
    }
}
