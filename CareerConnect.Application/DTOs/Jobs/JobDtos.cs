using CareerConnect.Domain.Enums;

namespace CareerConnect.Application.DTOs.Jobs;

public record JobDto(
    Guid Id,
    Guid CompanyId,
    string CompanyName,
    string CompanyLogoUrl,
    string Title,
    string Description,
    string Location,
    string JobType,
    decimal MinSalary,
    decimal MaxSalary,
    string Status,
    DateTime PostedAt
);

public record CreateJobRequest(
    string Title,
    string Description,
    string Location,
    string JobType,
    decimal MinSalary,
    decimal MaxSalary
);

public record UpdateJobRequest(
    string? Title,
    string? Description,
    string? Location,
    string? JobType,
    decimal? MinSalary,
    decimal? MaxSalary,
    JobStatus? Status
);
