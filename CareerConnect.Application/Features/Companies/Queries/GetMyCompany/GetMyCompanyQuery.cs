using CareerConnect.Application.DTOs.Companies;
using MediatR;

namespace CareerConnect.Application.Features.Companies.Queries.GetMyCompany;

public record GetMyCompanyQuery(Guid AdminUserId) : IRequest<CompanyDto>;
