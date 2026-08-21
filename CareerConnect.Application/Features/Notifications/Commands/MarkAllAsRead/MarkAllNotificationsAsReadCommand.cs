using System;
using MediatR;

namespace CareerConnect.Application.Features.Notifications.Commands.MarkAllAsRead;

public record MarkAllNotificationsAsReadCommand(Guid UserId) : IRequest<bool>;
