using CareerConnect.Application.DTOs.Companies;
using MediatR;
using System.Collections.Generic;

namespace CareerConnect.Application.Features.Companies.Queries.GetAllCompanies;

public record GetAllCompaniesQuery : IRequest<IEnumerable<CompanyDto>>;
