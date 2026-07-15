using CareerConnect.Domain.Enums;

namespace CareerConnect.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.JobSeeker;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public UserProfile? UserProfile { get; set; }
}
