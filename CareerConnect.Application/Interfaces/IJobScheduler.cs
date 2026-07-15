namespace CareerConnect.Application.Interfaces;

public interface IJobScheduler
{
    void ScheduleAiMatchScoring(Guid applicationId);
}
