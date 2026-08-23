using System;
using System.Threading.Tasks;
using CareerConnect.Application.Features.Messages.Commands.SendMessage;
using CareerConnect.Application.Features.Messages.Queries.GetMessages;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CareerConnect.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MessagesController : ControllerBase
{
    private readonly IMediator _mediator;

    public MessagesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{applicationId}")]
    public async Task<IActionResult> GetMessages(Guid applicationId)
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            return Unauthorized();

        var query = new GetMessagesQuery(applicationId, userId);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    public record SendMessageRequest(Guid ApplicationId, string Content);

    [HttpPost]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            return Unauthorized();

        var command = new SendMessageCommand(request.ApplicationId, userId, request.Content);
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}
