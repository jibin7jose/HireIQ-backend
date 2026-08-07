using System.Threading.Tasks;

namespace CareerConnect.Application.Interfaces;

public interface INotificationService
{
    Task SendNotificationAsync(string userId, string message, string type = "Info");
}
