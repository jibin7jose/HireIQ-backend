using CareerConnect.Application.DTOs.Companies;
using MediatR;
using System;

namespace CareerConnect.Application.Features.Companies.Queries.GetEmployerStats;

public record GetEmployerStatsQuery(Guid AdminUserId) : IRequest<EmployerStatsDto>;
