using CareerConnect.Application.DTOs.Auth;
using CareerConnect.Application.Interfaces;
using CareerConnect.Domain.Exceptions;
using MediatR;

namespace CareerConnect.Application.Features.Auth.Commands.Login;

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;

    public LoginCommandHandler(IUserRepository userRepository, ITokenService tokenService)
    {
        _userRepository = userRepository;
        _tokenService   = tokenService;
    }

    public async Task<AuthResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // 1. Fetch user
        var user = await _userRepository.GetByEmailAsync(
            request.Email.ToLowerInvariant(), cancellationToken)
            ?? throw new UnauthorizedException("Invalid email or password.");

        // 2. Verify password
        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedException("Invalid email or password.");

        // 3. Generate token
        var token = _tokenService.GenerateToken(user);

        var fullName = user.UserProfile?.FullName ?? string.Empty;

        return new AuthResponse(
            UserId:   user.Id,
            Email:    user.Email,
            FullName: fullName,
            Role:     user.Role.ToString(),
            Token:    token
        );
    }
}
