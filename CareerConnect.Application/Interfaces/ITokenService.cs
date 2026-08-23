using CareerConnect.Domain.Entities;

namespace CareerConnect.Application.Interfaces;

public interface ITokenService
{
    string GenerateToken(User user);
}
