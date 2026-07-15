using System.Security.Claims;
using CareerConnect.Application.DTOs.Applications;
using CareerConnect.Application.Features.Applications.Commands.ApplyToJob;
using CareerConnect.Application.Features.Applications.Commands.UpdateApplicationStatus;
using CareerConnect.Application.Features.Applications.Queries.GetMyApplications;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareerConnect.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class ApplicationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ApplicationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>POST /api/applications — JobSeeker only</summary>
    [HttpPost]
    [Authorize(Roles = "JobSeeker")]
    [ProducesResponseType(typeof(ApplicationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Apply(
        [FromBody] ApplyRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ApplyToJobCommand(
            JobId:       request.JobId,
            UserId:      GetCurrentUserId(),
            CoverLetter: request.CoverLetter,
            ResumeUrl:   request.ResumeUrl
        );

        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetMyApplications), result);
    }

    /// <summary>GET /api/applications/me — JobSeeker only</summary>
    [HttpGet("me")]
    [Authorize(Roles = "JobSeeker")]
    [ProducesResponseType(typeof(IEnumerable<ApplicationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyApplications(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetMyApplicationsQuery(GetCurrentUserId()), cancellationToken);
        return Ok(result);
    }

    /// <summary>PUT /api/applications/{id}/status — Employer only</summary>
    [HttpPut("{id:guid}/status")]
    [Authorize(Roles = "Employer")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateStatus(
        Guid id,
        [FromBody] UpdateApplicationStatusRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateApplicationStatusCommand(
            ApplicationId:    id,
            RequestingUserId: GetCurrentUserId(),
            Status:           request.Status
        );

        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    private Guid GetCurrentUserId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
               ?? User.FindFirstValue("sub")
               ?? throw new UnauthorizedAccessException("User ID not found in token.");
        return Guid.Parse(sub);
    }
}
