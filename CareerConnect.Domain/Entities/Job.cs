using CareerConnect.Domain.Enums;

namespace CareerConnect.Domain.Entities;

public class Job
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string JobType { get; set; } = string.Empty;   // e.g. "Full-Time", "Remote"
    public decimal MinSalary { get; set; }
    public decimal MaxSalary { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public JobStatus Status { get; set; } = JobStatus.Open;
    public DateTime PostedAt { get; set; }

    public Company? Company { get; set; }
    public ICollection<Application> Applications { get; set; } = new List<Application>();
}
