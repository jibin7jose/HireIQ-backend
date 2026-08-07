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
    private readonly IGeocodingService _geocodingService;
    private readonly IUnitOfWork _unitOfWork;

    public CreateJobCommandHandler(
        IJobRepository jobRepository,
        ICompanyRepository companyRepository,
        IGeocodingService geocodingService,
        IUnitOfWork unitOfWork)
    {
        _jobRepository     = jobRepository;
        _companyRepository = companyRepository;
        _geocodingService  = geocodingService;
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

        var coords = await _geocodingService.GetCoordinatesAsync(request.Location);
        if (coords.HasValue)
        {
            job.Latitude = coords.Value.Latitude;
            job.Longitude = coords.Value.Longitude;
        }

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
            PostedAt:       job.PostedAt,
            Latitude:       job.Latitude,
            Longitude:      job.Longitude
        );
    }
}
