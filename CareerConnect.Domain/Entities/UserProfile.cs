using System;
using System.Collections.Generic;

namespace CareerConnect.Domain.Entities;

public class UserProfile
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
    public string ResumeUrl { get; set; } = string.Empty;
    
    // AI Parsed Fields
    public List<string> Skills { get; set; } = new List<string>();
    public string ExperienceSummary { get; set; } = string.Empty;
    public string Education { get; set; } = string.Empty;
    public User? User { get; set; }
    public ICollection<Application> Applications { get; set; } = new List<Application>();
}
