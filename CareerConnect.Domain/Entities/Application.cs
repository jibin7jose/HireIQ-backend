using System;

namespace CareerConnect.Domain.Entities;

public class Application
{
    public Guid Id { get; set; }
    public Guid UserProfileId { get; set; }
    public Guid JobId { get; set; }
    public string Status { get; set; } = string.Empty;
    public int AiMatchScore { get; set; }
    public DateTime AppliedAt { get; set; }

    public UserProfile? UserProfile { get; set; }
    public Job? Job { get; set; }
}
