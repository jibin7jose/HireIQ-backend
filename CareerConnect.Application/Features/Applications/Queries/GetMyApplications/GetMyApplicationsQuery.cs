using CareerConnect.Application.DTOs.Applications;
using MediatR;

namespace CareerConnect.Application.Features.Applications.Queries.GetMyApplications;

public record GetMyApplicationsQuery(Guid UserId) : IRequest<IEnumerable<ApplicationDto>>;
