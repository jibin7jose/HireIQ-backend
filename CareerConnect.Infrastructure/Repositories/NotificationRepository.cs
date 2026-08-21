using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CareerConnect.Application.Interfaces;
using CareerConnect.Domain.Entities;
using CareerConnect.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CareerConnect.Infrastructure.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly ApplicationDbContext _context;

    public NotificationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Notification?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Notifications.FirstOrDefaultAsync(n => n.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Notification>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Notification>> GetUnreadByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        await _context.Notifications.AddAsync(notification, cancellationToken);
    }

    public Task UpdateAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        _context.Notifications.Update(notification);
        return Task.CompletedTask;
    }

    public async Task MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var unreadNotifications = await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync(cancellationToken);

        foreach (var notification in unreadNotifications)
        {
            notification.IsRead = true;
        }
        
        _context.Notifications.UpdateRange(unreadNotifications);
    }
}
