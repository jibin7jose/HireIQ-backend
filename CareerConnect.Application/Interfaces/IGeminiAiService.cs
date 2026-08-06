namespace CareerConnect.Application.Interfaces;

public interface IGeminiAiService
{
    Task<string> ParseResumeAsync(string resumeText, CancellationToken cancellationToken = default);
    Task<int> CalculateMatchScoreAsync(string jobDescription, string candidateSkills, string candidateExperience, CancellationToken cancellationToken = default);
    Task<Dictionary<Guid, int>> BatchCalculateMatchScoresAsync(IEnumerable<CareerConnect.Domain.Entities.Job> jobs, string candidateSkills, string candidateExperience, CancellationToken cancellationToken = default);
}
