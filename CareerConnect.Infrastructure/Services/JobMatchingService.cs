using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CareerConnect.Application.Interfaces;
using CareerConnect.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CareerConnect.Infrastructure.Services;

public class JobMatchingService : IJobMatchingService
{
    private readonly IJobRepository _jobRepository;
    private readonly IUserRepository _userRepository;
    private readonly IGeminiAiService _geminiAiService;
    private readonly IEmailService _emailService;
    private readonly ILogger<JobMatchingService> _logger;

    public JobMatchingService(
        IJobRepository jobRepository,
        IUserRepository userRepository,
        IGeminiAiService geminiAiService,
        IEmailService emailService,
        ILogger<JobMatchingService> logger)
    {
        _jobRepository = jobRepository;
        _userRepository = userRepository;
        _geminiAiService = geminiAiService;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task RunDailyJobAlertsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting daily job alerts matching process...");

        // 1. Get jobs posted in the last 24 hours
        var recentJobs = await _jobRepository.GetAllAsync(cancellationToken);
        recentJobs = recentJobs.Where(j => j.Status == JobStatus.Open && j.CreatedAt >= DateTime.UtcNow.AddDays(-1)).ToList();

        if (!recentJobs.Any())
        {
            _logger.LogInformation("No new jobs posted in the last 24 hours. Exiting job alerts process.");
            return;
        }

        // 2. Get all candidates who opted in for job alerts and have a parsed profile
        var allCandidates = await _userRepository.GetAllAsync(cancellationToken);
        var candidates = allCandidates.Where(u => 
            u.Role == UserRole.Candidate && 
            u.UserProfile != null && 
            u.UserProfile.ReceiveJobAlerts && 
            u.UserProfile.Skills.Any()
        ).ToList();

        if (!candidates.Any())
        {
            _logger.LogInformation("No candidates found eligible for job alerts.");
            return;
        }

        // 3. For each candidate, find the best jobs
        foreach (var candidate in candidates)
        {
            var profile = candidate.UserProfile!;
            var candidateSkillsStr = string.Join(", ", profile.Skills);
            var matchedJobCount = 0;
            var emailBody = $@"
                <h2>Your Daily AI Job Alerts</h2>
                <p>Hi {profile.FullName},</p>
                <p>We found some new jobs that match your profile!</p>
                <ul>
            ";

            foreach (var job in recentJobs)
            {
                // To save API calls, we could do keyword matching first, but for now we run AI scoring directly
                // In production, you'd want to batch or filter aggressively.
                try
                {
                    var score = await _geminiAiService.CalculateMatchScoreAsync(
                        job.Description,
                        candidateSkillsStr,
                        profile.ExperienceSummary,
                        cancellationToken);

                    if (score >= 70) // Arbitrary threshold for a "good match"
                    {
                        matchedJobCount++;
                        emailBody += $@"
                            <li>
                                <strong>{job.Title}</strong> at {job.Company?.Name ?? "Unknown Company"}<br/>
                                AI Match Score: <strong>{score}%</strong><br/>
                                <a href=""http://localhost:3000/candidate/jobs/{job.Id}"">View Job</a>
                            </li>
                        ";
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to calculate match score for candidate {CandidateId} and job {JobId}", candidate.Id, job.Id);
                }
            }

            if (matchedJobCount > 0)
            {
                emailBody += @"
                    </ul>
                    <p>Good luck!</p>
                    <p>The CareerConnect Team</p>
                ";

                await _emailService.SendEmailAsync(candidate.Email, "Your Top Job Matches for Today!", emailBody, cancellationToken);
                _logger.LogInformation("Sent job alerts to {CandidateEmail} with {MatchCount} matches.", candidate.Email, matchedJobCount);
            }
        }

        _logger.LogInformation("Finished daily job alerts matching process.");
    }
}
