using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CareerConnect.Application.Interfaces;
using CareerConnect.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace CareerConnect.Infrastructure.Auth;

public sealed class JwtTokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(User user)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var secret      = jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT Secret is not configured.");
        var issuer      = jwtSettings["Issuer"]  ?? "CareerConnect";
        var audience    = jwtSettings["Audience"] ?? "CareerConnectClient";
        var expiryMins  = int.Parse(jwtSettings["ExpiryMinutes"] ?? "1440");

        var key          = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials  = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub,   user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role,               user.Role.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer:             issuer,
            audience:           audience,
            claims:             claims,
            expires:            DateTime.UtcNow.AddMinutes(expiryMins),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
