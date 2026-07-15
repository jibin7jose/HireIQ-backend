using CareerConnect.Application.DTOs.Companies;
using CareerConnect.Application.Interfaces;
using CareerConnect.Domain.Entities;
using CareerConnect.Domain.Exceptions;
using MediatR;

namespace CareerConnect.Application.Features.Companies.Commands.CreateCompany;

public sealed class CreateCompanyCommandHandler : IRequestHandler<CreateCompanyCommand, CompanyDto>
{
    private readonly ICompanyRepository _companyRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCompanyCommandHandler(ICompanyRepository companyRepository, IUnitOfWork unitOfWork)
    {
        _companyRepository = companyRepository;
        _unitOfWork        = unitOfWork;
    }

    public async Task<CompanyDto> Handle(CreateCompanyCommand request, CancellationToken cancellationToken)
    {
        // One company per admin user
        var existing = await _companyRepository.GetByAdminUserIdAsync(request.AdminUserId, cancellationToken);
        if (existing is not null)
            throw new ConflictException("You have already registered a company.");

        var company = new Company
        {
            Id          = Guid.NewGuid(),
            AdminUserId = request.AdminUserId,
            Name        = request.Name,
            About       = request.About,
            Location    = request.Location,
            LogoUrl     = request.LogoUrl,
            IsVerified  = false
        };

        await _companyRepository.AddAsync(company, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CompanyDto(
            Id:         company.Id,
            Name:       company.Name,
            LogoUrl:    company.LogoUrl,
            About:      company.About,
            Location:   company.Location,
            IsVerified: company.IsVerified,
            JobCount:   0
        );
    }
}
