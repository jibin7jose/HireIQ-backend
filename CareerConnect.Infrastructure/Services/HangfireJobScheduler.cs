using CareerConnect.Application.Interfaces;
using Hangfire;

namespace CareerConnect.Infrastructure.Services;

public class HangfireJobScheduler : IJobScheduler
{
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly IRecurringJobManager _recurringJobManager;

    public HangfireJobScheduler(IBackgroundJobClient backgroundJobClient, IRecurringJobManager recurringJobManager)
    {
        _backgroundJobClient = backgroundJobClient;
        _recurringJobManager = recurringJobManager;
    }

    public void ScheduleAiMatchScoring(Guid applicationId)
    {
        _backgroundJobClient.Enqueue<IAiScoringService>(x => x.CalculateMatchScoreAsync(applicationId, default));
    }

    public void ScheduleDailyJobAlerts()
    {
        _recurringJobManager.AddOrUpdate<IJobMatchingService>(
            "daily-job-alerts",
            x => x.RunDailyJobAlertsAsync(default),
            Cron.Daily(9, 0)); // Run every day at 9 AM
    }
}
