using System;
using MediatR;
using System.Collections.Generic;
using CareerConnect.Application.DTOs.Notifications;

namespace CareerConnect.Application.Features.Notifications.Queries.GetMyNotifications;

public record GetMyNotificationsQuery(Guid UserId) : IRequest<IEnumerable<NotificationDto>>;
