using System;
using System.Threading.Tasks;
using CareerConnect.Application.Features.Notifications.Commands.MarkAllAsRead;
using CareerConnect.Application.Features.Notifications.Commands.MarkAsRead;
using CareerConnect.Application.Features.Notifications.Queries.GetMyNotifications;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareerConnect.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public NotificationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyNotifications()
    {
        var userId = GetUserId();
        var query = new GetMyNotificationsQuery(userId);
        var notifications = await _mediator.Send(query);
        return Ok(notifications);
    }

    [HttpPatch("{id}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        var userId = GetUserId();
        var command = new MarkNotificationAsReadCommand(id, userId);
        await _mediator.Send(command);
        return Ok();
    }

    [HttpPatch("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var userId = GetUserId();
        var command = new MarkAllNotificationsAsReadCommand(userId);
        await _mediator.Send(command);
        return Ok();
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim!);
    }
}
