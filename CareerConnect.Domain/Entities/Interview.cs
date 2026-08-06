using System;
using CareerConnect.Domain.Enums;

namespace CareerConnect.Domain.Entities;

public class Interview
{
    public Guid Id { get; set; }
    public Guid ApplicationId { get; set; }
    
    public DateTime ScheduledAt { get; set; }
    public int DurationMinutes { get; set; }
    public string MeetingLink { get; set; } = string.Empty;
    public InterviewStatus Status { get; set; } = InterviewStatus.Scheduled;

    public Application? Application { get; set; }
}
