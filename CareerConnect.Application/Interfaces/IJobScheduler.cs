namespace CareerConnect.Application.Interfaces;

public interface IJobScheduler
{
    void ScheduleAiMatchScoring(Guid applicationId);
    void ScheduleDailyJobAlerts();
    void ScheduleEmail(string to, string subject, string body);
}
