using System.Security.Claims;
using CareerConnect.Application.DTOs.Companies;
using CareerConnect.Application.Features.Companies.Commands.CreateCompany;
using CareerConnect.Application.Features.Companies.Queries.GetCompanyById;
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
