using CareerConnect.Application.DTOs.Companies;
using MediatR;

namespace CareerConnect.Application.Features.Companies.Queries.GetCompanyById;

public record GetCompanyByIdQuery(Guid Id) : IRequest<CompanyDto>;
