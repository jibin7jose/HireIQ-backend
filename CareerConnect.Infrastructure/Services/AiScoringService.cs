using CareerConnect.Application.Interfaces;
using CareerConnect.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace CareerConnect.Infrastructure.Services;

public class AiScoringService : IAiScoringService
{
    private readonly IApplicationRepository _applicationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AiScoringService> _logger;

    public AiScoringService(
        IApplicationRepository applicationRepository,
        IUnitOfWork unitOfWork,
        ILogger<AiScoringService> logger)
    {
        _applicationRepository = applicationRepository;
        _unitOfWork            = unitOfWork;
        _logger                = logger;
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

        // Simulate AI processing delay
        await Task.Delay(2000, cancellationToken);

        // Dummy scoring logic: random score between 50 and 100
        application.AiMatchScore = new Random().Next(50, 101);

        _applicationRepository.Update(application);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Finished AI match scoring for application {ApplicationId}. Score: {Score}", applicationId, application.AiMatchScore);
    }
}
