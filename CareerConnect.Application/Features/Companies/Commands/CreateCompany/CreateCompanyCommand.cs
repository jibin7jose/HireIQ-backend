using CareerConnect.Application.DTOs.Companies;
using MediatR;

namespace CareerConnect.Application.Features.Companies.Commands.CreateCompany;

public record CreateCompanyCommand(
    Guid AdminUserId,
    string Name,
    string About,
    string Location,
    string LogoUrl
) : IRequest<CompanyDto>;
