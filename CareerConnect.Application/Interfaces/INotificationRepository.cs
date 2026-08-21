using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CareerConnect.Domain.Entities;

namespace CareerConnect.Application.Interfaces;

public interface INotificationRepository
{
    Task<Notification?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Notification>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Notification>> GetUnreadByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(Notification notification, CancellationToken cancellationToken = default);
    Task UpdateAsync(Notification notification, CancellationToken cancellationToken = default);
    Task MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default);
}
