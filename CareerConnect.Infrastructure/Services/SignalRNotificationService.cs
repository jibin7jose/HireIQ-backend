using CareerConnect.Infrastructure.Hubs;
using CareerConnect.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace CareerConnect.Infrastructure.Services;

public class SignalRNotificationService : INotificationService
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public SignalRNotificationService(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task SendNotificationAsync(string userId, string message, string type = "Info")
    {
        await _hubContext.Clients.Group(userId).SendAsync("ReceiveNotification", new 
        { 
            Message = message, 
            Type = type,
            Timestamp = System.DateTime.UtcNow
        });
    }
}
