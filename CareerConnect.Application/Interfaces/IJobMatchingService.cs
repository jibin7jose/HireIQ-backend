using System.Threading;
using System.Threading.Tasks;

namespace CareerConnect.Application.Interfaces;

public interface IJobMatchingService
{
    Task RunDailyJobAlertsAsync(CancellationToken cancellationToken = default);
}
