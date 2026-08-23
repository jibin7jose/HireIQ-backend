using CareerConnect.Application.DTOs.Companies;
using CareerConnect.Application.Interfaces;
using CareerConnect.Domain.Entities;
using CareerConnect.Domain.Exceptions;
using MediatR;

namespace CareerConnect.Application.Features.Companies.Queries.GetCompanyById;

public sealed class GetCompanyByIdQueryHandler : IRequestHandler<GetCompanyByIdQuery, CompanyDto>
{
    private readonly ICompanyRepository _companyRepository;

    public GetCompanyByIdQueryHandler(ICompanyRepository companyRepository)
    {
        _companyRepository = companyRepository;
    }

    public async Task<CompanyDto> Handle(GetCompanyByIdQuery request, CancellationToken cancellationToken)
    {
        var company = await _companyRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Company), request.Id);

        return new CompanyDto(
            Id:         company.Id,
            Name:       company.Name,
            LogoUrl:    company.LogoUrl,
            About:      company.About,
            Location:   company.Location,
            IsVerified: company.IsVerified,
            JobCount:   company.Jobs.Count
        );
    }
}
