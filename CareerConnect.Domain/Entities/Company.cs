using System;
using System.Collections.Generic;

namespace CareerConnect.Domain.Entities;

public class Company
{
    public Guid Id { get; set; }
    public Guid AdminUserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string LogoUrl { get; set; } = string.Empty;
    public string About { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public bool IsVerified { get; set; }

    public User? AdminUser { get; set; }
    public ICollection<Job> Jobs { get; set; } = new List<Job>();
}
