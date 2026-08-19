using CareerConnect.Application.Interfaces;
using CareerConnect.Domain.Exceptions;
using MediatR;

namespace CareerConnect.Application.Features.Companies.Commands.UpdateCompany;

public sealed class UpdateCompanyCommandHandler : IRequestHandler<UpdateCompanyCommand>
{
    private readonly ICompanyRepository _companyRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCompanyCommandHandler(ICompanyRepository companyRepository, IUnitOfWork unitOfWork)
    {
        _companyRepository = companyRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateCompanyCommand request, CancellationToken cancellationToken)
    {
        var company = await _companyRepository.GetByAdminUserIdAsync(request.AdminUserId, cancellationToken)
            ?? throw new NotFoundException("Company", request.AdminUserId);

        company.Name = request.Name;
        company.About = request.About;
        company.Location = request.Location;
        company.LogoUrl = request.LogoUrl ?? string.Empty;

        _companyRepository.Update(company);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
