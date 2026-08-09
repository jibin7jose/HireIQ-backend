using CareerConnect.Application.Features.Admin.Queries.GetDashboardMetrics;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareerConnect.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("metrics")]
    public async Task<IActionResult> GetMetrics()
    {
        var result = await _mediator.Send(new GetDashboardMetricsQuery());
        return Ok(result);
    }
}
