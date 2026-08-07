using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CareerConnect.Application.DTOs.Applications;
using CareerConnect.Application.Interfaces;
using CareerConnect.Domain.Exceptions;
using MediatR;

namespace CareerConnect.Application.Features.Applications.Queries.GetCandidateApplications;

public record GetCandidateApplicationsQuery(Guid UserId) : IRequest<IEnumerable<CandidateApplicationDto>>;

public class CandidateApplicationDto
{
    public Guid Id { get; set; }
    public Guid JobId { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime AppliedAt { get; set; }
}

public class GetCandidateApplicationsQueryHandler : IRequestHandler<GetCandidateApplicationsQuery, IEnumerable<CandidateApplicationDto>>
{
    private readonly IApplicationRepository _applicationRepository;
    private readonly IUserRepository _userRepository;

    public GetCandidateApplicationsQueryHandler(
        IApplicationRepository applicationRepository,
        IUserRepository userRepository)
    {
        _applicationRepository = applicationRepository;
        _userRepository = userRepository;
    }

    public async Task<IEnumerable<CandidateApplicationDto>> Handle(GetCandidateApplicationsQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException("User", request.UserId);

        if (user.UserProfile == null)
            return new List<CandidateApplicationDto>();

        var applications = await _applicationRepository.GetByCandidateIdAsync(user.UserProfile.Id, cancellationToken);

        return applications.Select(a => new CandidateApplicationDto
        {
            Id = a.Id,
            JobId = a.JobId,
            JobTitle = a.Job?.Title ?? string.Empty,
            CompanyName = a.Job?.Company?.Name ?? string.Empty,
            Status = a.Status.ToString(),
            AppliedAt = a.AppliedAt
        });
    }
}
