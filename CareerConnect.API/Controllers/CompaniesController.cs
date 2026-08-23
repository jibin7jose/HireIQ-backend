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
    private readonly CareerConnect.Application.Interfaces.IStorageService _storageService;

    public CompaniesController(IMediator mediator, CareerConnect.Application.Interfaces.IStorageService storageService)
    {
        _mediator = mediator;
        _storageService = storageService;
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

    /// <summary>GET /api/companies/me — Employer only</summary>
    [HttpGet("me")]
    [Authorize(Roles = "Employer")]
    [ProducesResponseType(typeof(CompanyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyCompany(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await _mediator.Send(new CareerConnect.Application.Features.Companies.Queries.GetMyCompany.GetMyCompanyQuery(userId), cancellationToken);
        if (result == null) return NotFound();
        return Ok(result);
    }

    /// <summary>PUT /api/companies/me — Employer only</summary>
    [HttpPut("me")]
    [Authorize(Roles = "Employer")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateMyCompany([FromBody] UpdateCompanyRequest request, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var command = new CareerConnect.Application.Features.Companies.Commands.UpdateCompany.UpdateCompanyCommand(
            userId, request.Name, request.About, request.Location, request.LogoUrl);
            
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpPost("me/logo")]
    [Authorize(Roles = "Employer")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadLogo(IFormFile file, CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
            return BadRequest("File is empty or not provided.");

        if (!file.ContentType.StartsWith("image/"))
            return BadRequest("Only image files are allowed.");

        if (file.Length > 2 * 1024 * 1024)
            return BadRequest("File size cannot exceed 2MB.");

        using var stream = file.OpenReadStream();
        var publicUrl = await _storageService.UploadFileAsync(stream, file.FileName, file.ContentType, cancellationToken);
        
        return Ok(new { url = publicUrl });
    }

    private Guid GetCurrentUserId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
               ?? User.FindFirstValue("sub")
               ?? throw new UnauthorizedAccessException("User ID not found in token.");
        return Guid.Parse(sub);
    }
}
