using System;

namespace CareerConnect.Domain.Entities;

public class Notification
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = "Info"; // Info, Success, Warning, Error
    public bool IsRead { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public User? User { get; set; }
}
