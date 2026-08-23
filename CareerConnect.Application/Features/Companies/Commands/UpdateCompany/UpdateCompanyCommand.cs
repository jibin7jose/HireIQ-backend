using MediatR;

namespace CareerConnect.Application.Features.Companies.Commands.UpdateCompany;

public record UpdateCompanyCommand(
    Guid AdminUserId,
    string Name,
    string About,
    string Location,
    string? LogoUrl
) : IRequest;
