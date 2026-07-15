using CareerConnect.Application.DTOs.Auth;
using CareerConnect.Application.Features.Auth.Commands.Login;
using CareerConnect.Application.Features.Auth.Commands.Register;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CareerConnect.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>POST /api/auth/register</summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RegisterCommand(
            Email:    request.Email,
            Password: request.Password,
            FullName: request.FullName,
            Phone:    request.Phone,
            Role:     request.Role
        );

        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(Register), result);
    }

    /// <summary>POST /api/auth/login</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var command = new LoginCommand(request.Email, request.Password);
        var result  = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }
}
