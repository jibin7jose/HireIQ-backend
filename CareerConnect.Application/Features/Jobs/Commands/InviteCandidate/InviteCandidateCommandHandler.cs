using System;
using System.Threading;
using System.Threading.Tasks;
using CareerConnect.Application.Interfaces;
using CareerConnect.Domain.Entities;
using CareerConnect.Domain.Enums;
using CareerConnect.Domain.Exceptions;
using MediatR;
using System.Linq;

namespace CareerConnect.Application.Features.Jobs.Commands.InviteCandidate;

public class InviteCandidateCommandHandler : IRequestHandler<InviteCandidateCommand, Unit>
{
    private readonly IJobRepository _jobRepository;
    private readonly IUserRepository _userRepository;
    private readonly IApplicationRepository _applicationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public InviteCandidateCommandHandler(
        IJobRepository jobRepository,
        IUserRepository userRepository,
        IApplicationRepository applicationRepository,
        IUnitOfWork unitOfWork)
    {
        _jobRepository = jobRepository;
        _userRepository = userRepository;
        _applicationRepository = applicationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(InviteCandidateCommand request, CancellationToken cancellationToken)
    {
        var job = await _jobRepository.GetByIdAsync(request.JobId, cancellationToken)
            ?? throw new NotFoundException("Job", request.JobId);

        if (job.Company?.AdminUserId != request.EmployerId)
        {
            throw new UnauthorizedAccessException("You do not have permission to invite candidates for this job.");
        }

        var candidateUser = await _userRepository.GetByIdAsync(request.CandidateUserId, cancellationToken)
            ?? throw new NotFoundException("User", request.CandidateUserId);

        if (candidateUser.UserProfile == null)
        {
            throw new InvalidOperationException("Candidate profile not found.");
        }

        var existingApplications = await _applicationRepository.GetByJobIdAsync(request.JobId, cancellationToken);
        if (existingApplications.Any(a => a.UserProfileId == candidateUser.UserProfile.Id))
        {
            throw new InvalidOperationException("Candidate has already applied or been invited.");
        }

        var application = new CareerConnect.Domain.Entities.Application
        {
            JobId = request.JobId,
            UserProfileId = candidateUser.UserProfile.Id,
            Status = ApplicationStatus.Invited,
            AppliedAt = DateTime.UtcNow,
            AiMatchScore = request.AiMatchScore // Carry over the score they had in recommendations
        };

        await _applicationRepository.AddAsync(application, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
