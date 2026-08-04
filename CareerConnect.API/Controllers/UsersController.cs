using System.Security.Claims;
using CareerConnect.Application.Features.Users.Commands.UploadResume;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareerConnect.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly CareerConnect.Application.Interfaces.IUserRepository _userRepository;

    public UsersController(IMediator mediator, CareerConnect.Application.Interfaces.IUserRepository userRepository)
    {
        _mediator = mediator;
        _userRepository = userRepository;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
        {
            return Unauthorized();
        }

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null || user.UserProfile == null) return NotFound();

        return Ok(new 
        { 
            user.Email,
            user.Role,
            Profile = new 
            {
                user.UserProfile.FullName,
                user.UserProfile.Phone,
                user.UserProfile.ResumeUrl,
                user.UserProfile.Skills,
                user.UserProfile.ExperienceSummary,
                user.UserProfile.Education
            }
        });
    }

    [HttpPost("me/resume")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadResume(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("File is empty or not provided.");
        }

        if (file.ContentType != "application/pdf")
        {
            return BadRequest("Only PDF files are allowed.");
        }

        if (file.Length > 5 * 1024 * 1024) // 5 MB limit
        {
            return BadRequest("File size cannot exceed 5MB.");
        }

        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
        {
            return Unauthorized();
        }

        using var stream = file.OpenReadStream();
        
        var command = new UploadResumeCommand(
            UserId: userId,
            FileStream: stream,
            FileName: file.FileName,
            ContentType: file.ContentType
        );

        var publicUrl = await _mediator.Send(command);

        return Ok(new { url = publicUrl });
    }
}
