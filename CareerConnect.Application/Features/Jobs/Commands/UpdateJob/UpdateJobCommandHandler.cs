using CareerConnect.Application.Interfaces;
using CareerConnect.Domain.Entities;
using CareerConnect.Domain.Enums;
using CareerConnect.Domain.Exceptions;
using MediatR;

namespace CareerConnect.Application.Features.Jobs.Commands.UpdateJob;

public sealed class UpdateJobCommandHandler : IRequestHandler<UpdateJobCommand, Unit>
{
    private readonly IJobRepository _jobRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateJobCommandHandler(
        IJobRepository jobRepository,
        ICompanyRepository companyRepository,
        IUnitOfWork unitOfWork)
    {
        _jobRepository     = jobRepository;
        _companyRepository = companyRepository;
        _unitOfWork        = unitOfWork;
    }

    public async Task<Unit> Handle(UpdateJobCommand request, CancellationToken cancellationToken)
    {
        var job = await _jobRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Job), request.Id);

        // Ensure only the company admin can update
        var company = await _companyRepository.GetByIdAsync(job.CompanyId, cancellationToken);
        if (company is null || company.AdminUserId != request.RequestingUserId)
            throw new UnauthorizedException("You do not have permission to update this job.");

        if (request.Title is not null)       job.Title       = request.Title;
        if (request.Description is not null) job.Description = request.Description;
        if (request.Location is not null)    job.Location    = request.Location;
        if (request.JobType is not null)     job.JobType     = request.JobType;
        if (request.MinSalary.HasValue)      job.MinSalary   = request.MinSalary.Value;
        if (request.MaxSalary.HasValue)      job.MaxSalary   = request.MaxSalary.Value;
        if (request.Status.HasValue)         job.Status      = request.Status.Value;

        _jobRepository.Update(job);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
