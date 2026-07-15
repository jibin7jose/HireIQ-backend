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
    private readonly IUnitOfWork _unitOfWork;

    public UpdateApplicationStatusCommandHandler(
        IApplicationRepository applicationRepository,
        ICompanyRepository companyRepository,
        IUnitOfWork unitOfWork)
    {
        _applicationRepository = applicationRepository;
        _companyRepository     = companyRepository;
        _unitOfWork            = unitOfWork;
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

        return Unit.Value;
    }
}
