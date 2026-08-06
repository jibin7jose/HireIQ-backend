using System;
using System.Security.Claims;
using System.Threading.Tasks;
using CareerConnect.Application.Features.Interviews.Commands.ScheduleInterview;
using CareerConnect.Application.Features.Interviews.Queries.GetInterviewsForCandidate;
using CareerConnect.Application.Features.Interviews.Queries.GetInterviewsForEmployer;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareerConnect.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InterviewsController : ControllerBase
{
    private readonly IMediator _mediator;

    public InterviewsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [Authorize(Roles = "Employer")]
    public async Task<IActionResult> ScheduleInterview([FromBody] ScheduleInterviewRequest request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var command = new ScheduleInterviewCommand(request.ApplicationId, userId, request.ScheduledAt, request.DurationMinutes);
        var interviewId = await _mediator.Send(command);
        return Ok(new { Id = interviewId });
    }

    [HttpGet("candidate")]
    [Authorize(Roles = "Candidate")]
    public async Task<IActionResult> GetCandidateInterviews()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var query = new GetInterviewsForCandidateQuery(userId);
        var interviews = await _mediator.Send(query);
        return Ok(interviews);
    }

    [HttpGet("employer")]
    [Authorize(Roles = "Employer")]
    public async Task<IActionResult> GetEmployerInterviews()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var query = new GetInterviewsForEmployerQuery(userId);
        var interviews = await _mediator.Send(query);
        return Ok(interviews);
    }
}

public record ScheduleInterviewRequest(Guid ApplicationId, DateTime ScheduledAt, int DurationMinutes);
