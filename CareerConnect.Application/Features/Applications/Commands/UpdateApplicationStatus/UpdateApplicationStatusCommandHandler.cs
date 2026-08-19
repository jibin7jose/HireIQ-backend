using CareerConnect.Application.Interfaces;
using CareerConnect.Domain.Enums;
using CareerConnect.Domain.Exceptions;
using MediatR;
using JobApplication = CareerConnect.Domain.Entities.Application;

namespace CareerConnect.Application.Features.Applications.Commands.UpdateApplicationStatus;

public sealed class UpdateApplicationStatusCommandHandler
    : IRequestHandler<UpdateApplicationStatusCommand, Unit>
{
    private readonly IApplicationRepository _applicationRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJobScheduler _jobScheduler;
    private readonly INotificationService _notificationService;

    public UpdateApplicationStatusCommandHandler(
        IApplicationRepository applicationRepository,
        ICompanyRepository companyRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IJobScheduler jobScheduler,
        INotificationService notificationService)
    {
        _applicationRepository = applicationRepository;
        _companyRepository     = companyRepository;
        _userRepository        = userRepository;
        _unitOfWork            = unitOfWork;
        _jobScheduler          = jobScheduler;
        _notificationService   = notificationService;
    }

    public async Task<Unit> Handle(UpdateApplicationStatusCommand request, CancellationToken cancellationToken)
    {
        var application = await _applicationRepository.GetByIdAsync(request.ApplicationId, cancellationToken)
            ?? throw new NotFoundException("Application", request.ApplicationId);

        // Verify the requesting user is the employer who owns the job
        var company = await _companyRepository.GetByAdminUserIdAsync(request.RequestingUserId, cancellationToken)
            ?? throw new UnauthorizedException("Only company admins can update application status.");

        if (application.Job?.CompanyId != company.Id)
            throw new UnauthorizedException("You can only manage applications for your own jobs.");

        if (!Enum.TryParse<ApplicationStatus>(request.Status, ignoreCase: true, out var newStatus))
            throw new DomainException($"Invalid status '{request.Status}'.");

        application.Status = newStatus;
        _applicationRepository.Update(application);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Send Email Notification to Candidate
        if (application.UserProfile != null)
        {
            var candidate = await _userRepository.GetByIdAsync(application.UserProfile.UserId, cancellationToken);
            if (candidate != null)
            {
                var jobTitle = application.Job?.Title ?? "a job";
                var companyName = application.Job?.Company?.Name ?? "the employer";
                
                var subject = $"Application Status Update: {jobTitle}";
                var body = $@"
                    <h2>Your application status has been updated!</h2>
                    <p>Hi {application.UserProfile.FullName},</p>
                    <p><strong>{companyName}</strong> has updated the status of your application for <strong>{jobTitle}</strong>.</p>
                    <p>Your new status is: <strong>{newStatus.ToString()}</strong>.</p>
                    <p>Log in to your candidate dashboard to view more details.</p>
                ";
                _jobScheduler.ScheduleEmail(candidate.Email, subject, body);
                
                await _notificationService.SendNotificationAsync(candidate.Id.ToString(), $"Your application for {jobTitle} at {companyName} is now: {newStatus}", "Info");
            }
        }

        return Unit.Value;
    }
}
