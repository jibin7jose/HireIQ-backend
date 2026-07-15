using CareerConnect.Application.DTOs.Applications;
using CareerConnect.Application.Interfaces;
using CareerConnect.Domain.Entities;
using CareerConnect.Domain.Exceptions;
using MediatR;

namespace CareerConnect.Application.Features.Applications.Queries.GetMyApplications;

public sealed class GetMyApplicationsQueryHandler
    : IRequestHandler<GetMyApplicationsQuery, IEnumerable<ApplicationDto>>
{
    private readonly IApplicationRepository _applicationRepository;
    private readonly IUserRepository _userRepository;

    public GetMyApplicationsQueryHandler(
        IApplicationRepository applicationRepository,
        IUserRepository userRepository)
    {
        _applicationRepository = applicationRepository;
        _userRepository        = userRepository;
    }

    public async Task<IEnumerable<ApplicationDto>> Handle(
        GetMyApplicationsQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), request.UserId);

        var profile = user.UserProfile
            ?? throw new NotFoundException("User profile not found.");

        var applications = await _applicationRepository.GetByUserProfileIdAsync(profile.Id, cancellationToken);

        return applications.Select(a => new ApplicationDto(
            Id:            a.Id,
            JobId:         a.JobId,
            JobTitle:      a.Job?.Title ?? string.Empty,
            CompanyName:   a.Job?.Company?.Name ?? string.Empty,
            ApplicantName: profile.FullName,
            CoverLetter:   a.CoverLetter,
            ResumeUrl:     a.ResumeUrl,
            Status:        a.Status.ToString(),
            AiMatchScore:  a.AiMatchScore,
            AppliedAt:     a.AppliedAt
        ));
    }
}
