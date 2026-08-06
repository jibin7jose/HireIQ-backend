using System.Security.Claims;
using CareerConnect.Application.DTOs;
using CareerConnect.Application.DTOs.Jobs;
using CareerConnect.Application.Features.Jobs.Commands.CreateJob;
using CareerConnect.Application.Features.Jobs.Commands.DeleteJob;
using CareerConnect.Application.Features.Jobs.Commands.UpdateJob;
using CareerConnect.Application.Features.Jobs.Queries.GetAllJobs;
using CareerConnect.Application.Features.Jobs.Queries.GetJobById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareerConnect.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class JobsController : ControllerBase
{
    private readonly IMediator _mediator;

    public JobsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>GET /api/jobs?keyword=&location=&jobType=&minSalary=&maxSalary=&pageNumber=&pageSize=</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<JobDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? keyword,
        [FromQuery] string? location,
        [FromQuery] string? jobType,
        [FromQuery] decimal? minSalary,
        [FromQuery] decimal? maxSalary,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAllJobsQuery(location, jobType, keyword, minSalary, maxSalary, pageNumber, pageSize);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>GET /api/jobs/{id}</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(JobDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetJobByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    /// <summary>GET /api/jobs/recommended — Candidate only</summary>
    [HttpGet("recommended")]
    [Authorize(Roles = "Candidate")]
    [ProducesResponseType(typeof(IEnumerable<JobDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRecommended(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CareerConnect.Application.Features.Jobs.Queries.GetRecommendedJobs.GetRecommendedJobsQuery(GetCurrentUserId()), cancellationToken);
        return Ok(result);
    }

    /// <summary>POST /api/jobs — Employer only</summary>
    [HttpPost]
    [Authorize(Roles = "Employer")]
    [ProducesResponseType(typeof(JobDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create(
        [FromBody] CreateJobRequest request,
        CancellationToken cancellationToken)
    {
        var companyId = GetCurrentUserId(); // Company fetched by admin user id in handler

        var command = new CreateJobCommand(
            CompanyId:   companyId,
            Title:       request.Title,
            Description: request.Description,
            Location:    request.Location,
            JobType:     request.JobType,
            MinSalary:   request.MinSalary,
            MaxSalary:   request.MaxSalary
        );

        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>PUT /api/jobs/{id} — Employer only</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Employer")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateJobRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateJobCommand(
            Id:               id,
            RequestingUserId: GetCurrentUserId(),
            Title:            request.Title,
            Description:      request.Description,
            Location:         request.Location,
            JobType:          request.JobType,
            MinSalary:        request.MinSalary,
            MaxSalary:        request.MaxSalary,
            Status:           request.Status
        );

        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>DELETE /api/jobs/{id} — Employer only</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Employer")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteJobCommand(id, GetCurrentUserId()), cancellationToken);
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
