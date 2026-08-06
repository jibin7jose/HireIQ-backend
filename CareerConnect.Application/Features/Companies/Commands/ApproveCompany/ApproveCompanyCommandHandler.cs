using CareerConnect.Application.Interfaces;
using CareerConnect.Domain.Entities;
using CareerConnect.Domain.Exceptions;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace CareerConnect.Application.Features.Companies.Commands.ApproveCompany;

public sealed class ApproveCompanyCommandHandler : IRequestHandler<ApproveCompanyCommand>
{
    private readonly ICompanyRepository _companyRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ApproveCompanyCommandHandler(ICompanyRepository companyRepository, IUnitOfWork unitOfWork)
    {
        _companyRepository = companyRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ApproveCompanyCommand request, CancellationToken cancellationToken)
    {
        var company = await _companyRepository.GetByIdAsync(request.CompanyId, cancellationToken)
            ?? throw new NotFoundException(nameof(Company), request.CompanyId);

        if (!company.IsVerified)
        {
            company.IsVerified = true;
            _companyRepository.Update(company);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
