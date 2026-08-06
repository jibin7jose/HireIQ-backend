using CareerConnect.Domain.Enums;

namespace CareerConnect.Domain.Entities;

public class Application
{
    public Guid Id { get; set; }
    public Guid UserProfileId { get; set; }
    public Guid JobId { get; set; }
    public string CoverLetter { get; set; } = string.Empty;
    public string ResumeUrl { get; set; } = string.Empty;
    public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;
    public int AiMatchScore { get; set; }
    public DateTime AppliedAt { get; set; }

    public UserProfile? UserProfile { get; set; }
    public Job? Job { get; set; }
    public ICollection<Interview> Interviews { get; set; } = new List<Interview>();
}
