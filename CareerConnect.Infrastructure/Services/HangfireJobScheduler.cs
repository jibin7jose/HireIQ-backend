using CareerConnect.Application.Interfaces;
using Hangfire;

namespace CareerConnect.Infrastructure.Services;

public class HangfireJobScheduler : IJobScheduler
{
    private readonly IBackgroundJobClient _backgroundJobClient;

    public HangfireJobScheduler(IBackgroundJobClient backgroundJobClient)
    {
        _backgroundJobClient = backgroundJobClient;
    }

    public void ScheduleAiMatchScoring(Guid applicationId)
    {
        _backgroundJobClient.Enqueue<IAiScoringService>(x => x.CalculateMatchScoreAsync(applicationId, CancellationToken.None));
    }
}
