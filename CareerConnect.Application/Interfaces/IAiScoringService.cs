namespace CareerConnect.Application.Interfaces;

public interface IAiScoringService
{
    Task CalculateMatchScoreAsync(Guid applicationId, CancellationToken cancellationToken = default);
}
