using MediatR;
using System;

namespace CareerConnect.Application.Features.Companies.Commands.ApproveCompany;

public record ApproveCompanyCommand(Guid CompanyId) : IRequest;
