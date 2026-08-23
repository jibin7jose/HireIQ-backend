using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CareerConnect.Domain.Entities;

namespace CareerConnect.Application.Interfaces;

public interface IMessageRepository
{
    Task<Message?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Message>> GetMessagesByApplicationIdAsync(Guid applicationId, CancellationToken cancellationToken = default);
    Task AddAsync(Message message, CancellationToken cancellationToken = default);
    void Update(Message message);
}
