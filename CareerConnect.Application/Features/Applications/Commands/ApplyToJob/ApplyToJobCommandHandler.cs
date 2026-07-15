using CareerConnect.Application.DTOs.Applications;
using CareerConnect.Application.Interfaces;
using CareerConnect.Domain.Entities;
using CareerConnect.Domain.Enums;
using CareerConnect.Domain.Exceptions;
using MediatR;
using JobApplication = CareerConnect.Domain.Entities.Application;

namespace CareerConnect.Application.Features.Applications.Commands.ApplyToJob;

public sealed class ApplyToJobCommandHandler : IRequestHandler<ApplyToJobCommand, ApplicationDto>
{
    private readonly IApplicationRepository _applicationRepository;
    private readonly IJobRepository _jobRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJobScheduler _jobScheduler;

    public ApplyToJobCommandHandler(
        IApplicationRepository applicationRepository,
        IJobRepository jobRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IJobScheduler jobScheduler)
    {
        _applicationRepository = applicationRepository;
        _jobRepository         = jobRepository;
        _userRepository        = userRepository;
        _unitOfWork            = unitOfWork;
        _jobScheduler          = jobScheduler;
    }

    public async Task<ApplicationDto> Handle(ApplyToJobCommand request, CancellationToken cancellationToken)
    {
        // 1. Validate job exists and is open
        var job = await _jobRepository.GetByIdAsync(request.JobId, cancellationToken)
            ?? throw new NotFoundException(nameof(Job), request.JobId);

        if (job.Status != JobStatus.Open)
            throw new DomainException("This job is no longer accepting applications.");

        // 2. Validate user and profile
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), request.UserId);

        var profile = user.UserProfile
            ?? throw new NotFoundException("User profile not found. Please complete your profile first.");

        // 3. Check duplicate application
        if (await _applicationRepository.ExistsAsync(profile.Id, request.JobId, cancellationToken))
            throw new ConflictException("You have already applied to this job.");

        // 4. Create application
        var application = new JobApplication
        {
            Id            = Guid.NewGuid(),
            UserProfileId = profile.Id,
            JobId         = request.JobId,
            CoverLetter   = request.CoverLetter,
            ResumeUrl     = request.ResumeUrl,
            Status        = ApplicationStatus.Pending,
            AiMatchScore  = 0,   // AI scoring can be triggered asynchronously via Hangfire
            AppliedAt     = DateTime.UtcNow
        };

        await _applicationRepository.AddAsync(application, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Schedule AI match scoring in background
        _jobScheduler.ScheduleAiMatchScoring(application.Id);

        return new ApplicationDto(
            Id:            application.Id,
            JobId:         job.Id,
            JobTitle:      job.Title,
            CompanyName:   job.Company?.Name ?? string.Empty,
            ApplicantName: profile.FullName,
            CoverLetter:   application.CoverLetter,
            ResumeUrl:     application.ResumeUrl,
            Status:        application.Status.ToString(),
            AiMatchScore:  application.AiMatchScore,
            AppliedAt:     application.AppliedAt
        );
    }
}
