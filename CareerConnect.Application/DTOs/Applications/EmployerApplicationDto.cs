using CareerConnect.Domain.Enums;

namespace CareerConnect.Application.DTOs.Applications;

public class EmployerApplicationDto
{
    public Guid Id { get; set; }
    public Guid JobId { get; set; }
    public Guid UserProfileId { get; set; }
    public string CandidateName { get; set; } = string.Empty;
    public string CandidateEmail { get; set; } = string.Empty;
    public string CandidatePhone { get; set; } = string.Empty;
    public List<string> CandidateSkills { get; set; } = new();
    public string CoverLetter { get; set; } = string.Empty;
    public string ResumeUrl { get; set; } = string.Empty;
    public ApplicationStatus Status { get; set; }
    public int AiMatchScore { get; set; }
    public DateTime AppliedAt { get; set; }
    
    // Interview info
    public string? MeetingLink { get; set; }
    public DateTime? InterviewDate { get; set; }
}
