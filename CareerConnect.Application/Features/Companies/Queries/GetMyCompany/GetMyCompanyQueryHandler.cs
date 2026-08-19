using CareerConnect.Application.DTOs.Companies;
using CareerConnect.Application.Interfaces;
using CareerConnect.Domain.Exceptions;
using MediatR;

namespace CareerConnect.Application.Features.Companies.Queries.GetMyCompany;

public sealed class GetMyCompanyQueryHandler : IRequestHandler<GetMyCompanyQuery, CompanyDto>
{
    private readonly ICompanyRepository _companyRepository;

    public GetMyCompanyQueryHandler(ICompanyRepository companyRepository)
    {
        _companyRepository = companyRepository;
    }

    public async Task<CompanyDto> Handle(GetMyCompanyQuery request, CancellationToken cancellationToken)
    {
        var company = await _companyRepository.GetByAdminUserIdAsync(request.AdminUserId, cancellationToken)
            ?? throw new NotFoundException("Company", request.AdminUserId);

        return new CompanyDto(
            Id:         company.Id,
            Name:       company.Name,
            LogoUrl:    company.LogoUrl,
            About:      company.About,
            Location:   company.Location,
            IsVerified: company.IsVerified,
            JobCount:   company.Jobs?.Count ?? 0
        );
    }
}
