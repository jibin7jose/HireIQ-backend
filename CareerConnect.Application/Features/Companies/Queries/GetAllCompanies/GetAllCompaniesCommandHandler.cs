using CareerConnect.Application.DTOs.Companies;
using CareerConnect.Application.Interfaces;
using MediatR;
using System.Linq;

namespace CareerConnect.Application.Features.Companies.Queries.GetAllCompanies;

public sealed class GetAllCompaniesCommandHandler : IRequestHandler<GetAllCompaniesQuery, IEnumerable<CompanyDto>>
{
    private readonly ICompanyRepository _companyRepository;

    public GetAllCompaniesCommandHandler(ICompanyRepository companyRepository)
    {
        _companyRepository = companyRepository;
    }

    public async Task<IEnumerable<CompanyDto>> Handle(GetAllCompaniesQuery request, CancellationToken cancellationToken)
    {
        var companies = await _companyRepository.GetAllAsync(cancellationToken);

        return companies.Select(company => new CompanyDto(
            Id:         company.Id,
            Name:       company.Name,
            LogoUrl:    company.LogoUrl,
            About:      company.About,
            Location:   company.Location,
            IsVerified: company.IsVerified,
            JobCount:   company.Jobs.Count
        ));
    }
}
