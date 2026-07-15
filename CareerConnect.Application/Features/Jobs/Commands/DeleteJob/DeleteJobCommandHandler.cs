using CareerConnect.Application.Interfaces;
using CareerConnect.Domain.Entities;
using CareerConnect.Domain.Exceptions;
using MediatR;

namespace CareerConnect.Application.Features.Jobs.Commands.DeleteJob;

public sealed class DeleteJobCommandHandler : IRequestHandler<DeleteJobCommand, Unit>
{
    private readonly IJobRepository _jobRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteJobCommandHandler(
        IJobRepository jobRepository,
        ICompanyRepository companyRepository,
        IUnitOfWork unitOfWork)
    {
        _jobRepository     = jobRepository;
        _companyRepository = companyRepository;
        _unitOfWork        = unitOfWork;
    }

    public async Task<Unit> Handle(DeleteJobCommand request, CancellationToken cancellationToken)
    {
        var job = await _jobRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Job), request.Id);

        var company = await _companyRepository.GetByIdAsync(job.CompanyId, cancellationToken);
        if (company is null || company.AdminUserId != request.RequestingUserId)
            throw new UnauthorizedException("You do not have permission to delete this job.");

        _jobRepository.Delete(job);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
