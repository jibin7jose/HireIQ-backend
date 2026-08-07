using System;
using System.Threading;
using System.Threading.Tasks;
using CareerConnect.Application.Interfaces;
using CareerConnect.Domain.Entities;
using CareerConnect.Domain.Enums;
using CareerConnect.Domain.Exceptions;
using MediatR;

namespace CareerConnect.Application.Features.Interviews.Commands.ScheduleInterview;

public record ScheduleInterviewCommand(
    Guid ApplicationId,
    Guid RequestingUserId,
    DateTime ScheduledAt,
    int DurationMinutes
) : IRequest<Guid>;

public class ScheduleInterviewCommandHandler : IRequestHandler<ScheduleInterviewCommand, Guid>
{
    private readonly IApplicationRepository _applicationRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly IInterviewRepository _interviewRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly INotificationService _notificationService;

    public ScheduleInterviewCommandHandler(
        IApplicationRepository applicationRepository,
        ICompanyRepository companyRepository,
        IInterviewRepository interviewRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        INotificationService notificationService)
    {
        _applicationRepository = applicationRepository;
        _companyRepository     = companyRepository;
        _interviewRepository   = interviewRepository;
        _userRepository        = userRepository;
        _unitOfWork            = unitOfWork;
        _emailService          = emailService;
        _notificationService   = notificationService;
    }

    public async Task<Guid> Handle(ScheduleInterviewCommand request, CancellationToken cancellationToken)
    {
        var application = await _applicationRepository.GetByIdAsync(request.ApplicationId, cancellationToken)
            ?? throw new NotFoundException("Application", request.ApplicationId);

        var company = await _companyRepository.GetByAdminUserIdAsync(request.RequestingUserId, cancellationToken)
            ?? throw new UnauthorizedException("Only company admins can schedule interviews.");

        if (application.Job?.CompanyId != company.Id)
            throw new UnauthorizedException("You can only schedule interviews for your own jobs.");

        var interviewId = Guid.NewGuid();
        var meetingLink = $"/interviews/{interviewId}";

        var interview = new Interview
        {
            Id = interviewId,
            ApplicationId = request.ApplicationId,
            ScheduledAt = request.ScheduledAt,
            DurationMinutes = request.DurationMinutes,
            MeetingLink = meetingLink,
            Status = InterviewStatus.Scheduled
        };

        await _interviewRepository.AddAsync(interview, cancellationToken);
        
        // Also update the application status to Interviewing if it was Pending or Reviewed
        if (application.Status != ApplicationStatus.Hired && application.Status != ApplicationStatus.Rejected)
        {
            application.Status = ApplicationStatus.Shortlisted;
            _applicationRepository.Update(application);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Send Email to Candidate
        if (application.UserProfile != null)
        {
            var candidate = await _userRepository.GetByIdAsync(application.UserProfile.UserId, cancellationToken);
            if (candidate != null)
            {
                var jobTitle = application.Job?.Title ?? "a job";
                
                var subject = $"Interview Scheduled: {company.Name} - {jobTitle}";
                var body = $@"
                    <h2>Great news! You have an interview scheduled.</h2>
                    <p>Hi {application.UserProfile.FullName},</p>
                    <p><strong>{company.Name}</strong> has scheduled a {request.DurationMinutes}-minute interview with you for the <strong>{jobTitle}</strong> position.</p>
                    <p><strong>Date & Time:</strong> {request.ScheduledAt.ToString("f")}</p>
                    <p><strong>Meeting Link:</strong> <a href='{meetingLink}'>{meetingLink}</a></p>
                    <p>Log in to your dashboard to view more details.</p>
                ";
                await _emailService.SendEmailAsync(candidate.Email, subject, body, cancellationToken);
                
                await _notificationService.SendNotificationAsync(candidate.Id.ToString(), $"You have an interview scheduled for {jobTitle} at {company.Name} on {request.ScheduledAt.ToString("f")}", "Info");
            }
        }

        return interviewId;
    }
}
