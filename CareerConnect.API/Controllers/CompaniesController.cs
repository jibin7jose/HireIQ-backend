using System.Security.Claims;
using CareerConnect.Application.DTOs.Companies;
using CareerConnect.Application.Features.Companies.Commands.CreateCompany;
using CareerConnect.Application.Features.Companies.Queries.GetCompanyById;
using CareerConnect.Application.Features.Companies.Queries.GetAllCompanies;
using CareerConnect.Application.Features.Companies.Queries.GetEmployerStats;
using CareerConnect.Application.Features.Companies.Commands.ApproveCompany;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareerConnect.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class CompaniesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CompaniesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>GET /api/companies/{id}</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CompanyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCompanyByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    /// <summary>GET /api/companies — Admin only</summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(IEnumerable<CompanyDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetAllCompaniesQuery(), cancellationToken);
        return Ok(result);
    }

    /// <summary>POST /api/companies/{id}/approve — Admin only</summary>
    [HttpPost("{id:guid}/approve")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Approve(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new ApproveCompanyCommand(id), cancellationToken);
        return NoContent();
    }

    /// <summary>GET /api/companies/stats — Employer only</summary>
    [HttpGet("stats")]
    [Authorize(Roles = "Employer")]
    [ProducesResponseType(typeof(EmployerStatsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStats(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await _mediator.Send(new GetEmployerStatsQuery(userId), cancellationToken);
        return Ok(result);
    }

    /// <summary>POST /api/companies — Employer only</summary>
    [HttpPost]
    [Authorize(Roles = "Employer")]
    [ProducesResponseType(typeof(CompanyDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateCompanyRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        var command = new CreateCompanyCommand(
            AdminUserId: userId,
            Name:        request.Name,
            About:       request.About,
            Location:    request.Location,
            LogoUrl:     request.LogoUrl
        );

        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    private Guid GetCurrentUserId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
               ?? User.FindFirstValue("sub")
               ?? throw new UnauthorizedAccessException("User ID not found in token.");
        return Guid.Parse(sub);
    }
}
