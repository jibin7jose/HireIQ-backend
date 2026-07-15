using CareerConnect.Application.Interfaces;
using CareerConnect.Domain.Entities;
using CareerConnect.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CareerConnect.Infrastructure.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Users
            .Include(u => u.UserProfile)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        => await _context.Users
            .Include(u => u.UserProfile)
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

    public async Task<bool> ExistsAsync(string email, CancellationToken cancellationToken = default)
        => await _context.Users.AnyAsync(u => u.Email == email, cancellationToken);

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
        => await _context.Users.AddAsync(user, cancellationToken);
}
