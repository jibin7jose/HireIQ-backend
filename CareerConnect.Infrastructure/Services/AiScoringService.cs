using CareerConnect.Application.Interfaces;
using CareerConnect.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace CareerConnect.Infrastructure.Services;

public class AiScoringService : IAiScoringService
{
    private readonly IApplicationRepository _applicationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AiScoringService> _logger;
    private readonly IGeminiAiService _geminiAiService;
    private readonly IJobRepository _jobRepository;

    public AiScoringService(
        IApplicationRepository applicationRepository,
        IUnitOfWork unitOfWork,
        ILogger<AiScoringService> logger,
        IGeminiAiService geminiAiService,
        IJobRepository jobRepository)
    {
        _applicationRepository = applicationRepository;
        _unitOfWork            = unitOfWork;
        _logger                = logger;
        _geminiAiService       = geminiAiService;
        _jobRepository         = jobRepository;
    }

    public async Task CalculateMatchScoreAsync(Guid applicationId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting AI match scoring for application {ApplicationId}", applicationId);

        var application = await _applicationRepository.GetByIdAsync(applicationId, cancellationToken);
        if (application == null)
        {
            _logger.LogWarning("Application {ApplicationId} not found during AI scoring.", applicationId);
            return;
        }

        var job = await _jobRepository.GetByIdAsync(application.JobId, cancellationToken);
        var profile = application.UserProfile;

        if (job == null || profile == null)
        {
            _logger.LogWarning("Missing Job or Profile for Application {ApplicationId}", applicationId);
            return;
        }

        try
        {
            var skills = profile.Skills != null ? string.Join(", ", profile.Skills) : "";
            var score = await _geminiAiService.CalculateMatchScoreAsync(
                job.Description,
                skills,
                profile.ExperienceSummary ?? "",
                cancellationToken);

            application.AiMatchScore = score;
            _applicationRepository.Update(application);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Finished AI match scoring for application {ApplicationId}. Score: {Score}", applicationId, application.AiMatchScore);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating AI Match Score for application {ApplicationId}", applicationId);
        }
    }
}
