using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CareerConnect.Application.DTOs.Notifications;
using CareerConnect.Application.Interfaces;
using MediatR;

namespace CareerConnect.Application.Features.Notifications.Queries.GetMyNotifications;

public sealed class GetMyNotificationsQueryHandler : IRequestHandler<GetMyNotificationsQuery, IEnumerable<NotificationDto>>
{
    private readonly INotificationRepository _notificationRepository;

    public GetMyNotificationsQueryHandler(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task<IEnumerable<NotificationDto>> Handle(GetMyNotificationsQuery request, CancellationToken cancellationToken)
    {
        var notifications = await _notificationRepository.GetByUserIdAsync(request.UserId, cancellationToken);

        return notifications.Select(n => new NotificationDto(
            Id: n.Id,
            Message: n.Message,
            Type: n.Type,
            IsRead: n.IsRead,
            Timestamp: n.CreatedAt
        ));
    }
}
