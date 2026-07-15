using CareerConnect.Application.DTOs.Jobs;
using CareerConnect.Application.Interfaces;
using CareerConnect.Domain.Entities;
using CareerConnect.Domain.Enums;
using CareerConnect.Domain.Exceptions;
using MediatR;

namespace CareerConnect.Application.Features.Jobs.Commands.CreateJob;

public sealed class CreateJobCommandHandler : IRequestHandler<CreateJobCommand, JobDto>
{
    private readonly IJobRepository _jobRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateJobCommandHandler(
        IJobRepository jobRepository,
        ICompanyRepository companyRepository,
        IUnitOfWork unitOfWork)
    {
        _jobRepository     = jobRepository;
        _companyRepository = companyRepository;
        _unitOfWork        = unitOfWork;
    }

    public async Task<JobDto> Handle(CreateJobCommand request, CancellationToken cancellationToken)
    {
        var company = await _companyRepository.GetByIdAsync(request.CompanyId, cancellationToken)
            ?? throw new NotFoundException(nameof(Company), request.CompanyId);

        var job = new Job
        {
            Id          = Guid.NewGuid(),
            CompanyId   = request.CompanyId,
            Title       = request.Title,
            Description = request.Description,
            Location    = request.Location,
            JobType     = request.JobType,
            MinSalary   = request.MinSalary,
            MaxSalary   = request.MaxSalary,
            Status      = JobStatus.Open,
            PostedAt    = DateTime.UtcNow
        };

        await _jobRepository.AddAsync(job, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new JobDto(
            Id:             job.Id,
            CompanyId:      job.CompanyId,
            CompanyName:    company.Name,
            CompanyLogoUrl: company.LogoUrl,
            Title:          job.Title,
            Description:    job.Description,
            Location:       job.Location,
            JobType:        job.JobType,
            MinSalary:      job.MinSalary,
            MaxSalary:      job.MaxSalary,
            Status:         job.Status.ToString(),
            PostedAt:       job.PostedAt
        );
    }
}
