using System.Security.Claims;
using CareerConnect.Application.DTOs.Applications;
using CareerConnect.Application.Features.Applications.Commands.ApplyToJob;
using CareerConnect.Application.Features.Applications.Commands.UpdateApplicationStatus;
using CareerConnect.Application.Features.Applications.Queries.GetApplicationsForJob;
using CareerConnect.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareerConnect.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ApplicationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ApplicationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>POST /api/applications — Candidate only</summary>
    [HttpPost]
    [Authorize(Roles = "Candidate")]
    [ProducesResponseType(typeof(ApplicationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Apply([FromBody] ApplyRequest request, CancellationToken cancellationToken)
    {
        var command = new ApplyToJobCommand(request.JobId, GetCurrentUserId(), request.CoverLetter ?? string.Empty, request.ResumeUrl ?? string.Empty);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>GET /api/applications/job/{jobId} — Employer only</summary>
    [HttpGet("job/{jobId:guid}")]
    [Authorize(Roles = "Employer")]
    [ProducesResponseType(typeof(List<EmployerApplicationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetForJob(Guid jobId, CancellationToken cancellationToken)
    {
        var query = new GetApplicationsForJobQuery(jobId, GetCurrentUserId());
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>PUT /api/applications/{id}/status — Employer only</summary>
    [HttpPut("{id:guid}/status")]
    [Authorize(Roles = "Employer")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateStatusRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateApplicationStatusCommand(id, GetCurrentUserId(), request.Status.ToString());
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

public class ApplyJobRequest
{
    public Guid JobId { get; set; }
    public string? CoverLetter { get; set; }
}

public class UpdateStatusRequest
{
    public ApplicationStatus Status { get; set; }
}
