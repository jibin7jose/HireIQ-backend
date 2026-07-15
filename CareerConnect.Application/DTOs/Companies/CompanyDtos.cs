namespace CareerConnect.Application.DTOs.Companies;

public record CompanyDto(
    Guid Id,
    string Name,
    string LogoUrl,
    string About,
    string Location,
    bool IsVerified,
    int JobCount
);

public record CreateCompanyRequest(
    string Name,
    string About,
    string Location,
    string LogoUrl
);

public record UpdateCompanyRequest(
    string? Name,
    string? About,
    string? Location,
    string? LogoUrl
);
