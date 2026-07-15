using CareerConnect.Application.DTOs.Auth;
using CareerConnect.Application.Interfaces;
using CareerConnect.Domain.Entities;
using CareerConnect.Domain.Exceptions;
using MediatR;
using BCrypt.Net;

namespace CareerConnect.Application.Features.Auth.Commands.Register;

public sealed class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;

    public RegisterCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ITokenService tokenService)
    {
        _userRepository = userRepository;
        _unitOfWork     = unitOfWork;
        _tokenService   = tokenService;
    }

    public async Task<AuthResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        // 1. Guard: duplicate email
        if (await _userRepository.ExistsAsync(request.Email, cancellationToken))
            throw new ConflictException($"A user with email '{request.Email}' already exists.");

        // 2. Create User
        var user = new User
        {
            Id           = Guid.NewGuid(),
            Email        = request.Email.ToLowerInvariant(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role         = request.Role,
            CreatedAt    = DateTime.UtcNow
        };

        // 3. Create linked UserProfile
        var profile = new UserProfile
        {
            Id       = Guid.NewGuid(),
            UserId   = user.Id,
            FullName = request.FullName,
            Phone    = request.Phone
        };

        user.UserProfile = profile;

        await _userRepository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 4. Generate JWT
        var token = _tokenService.GenerateToken(user);

        return new AuthResponse(
            UserId:   user.Id,
            Email:    user.Email,
            FullName: profile.FullName,
            Role:     user.Role.ToString(),
            Token:    token
        );
    }
}
