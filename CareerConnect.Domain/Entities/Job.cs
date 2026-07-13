using System;
using System.Collections.Generic;

namespace CareerConnect.Domain.Entities;

public class Job
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal MinSalary { get; set; }
    public decimal MaxSalary { get; set; }
    public string Location { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime PostedAt { get; set; }

    public Company? Company { get; set; }
    public ICollection<Application> Applications { get; set; } = new List<Application>();
}
