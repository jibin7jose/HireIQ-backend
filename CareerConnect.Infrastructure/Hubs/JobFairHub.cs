using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace CareerConnect.Infrastructure.Hubs;

public class JobFairHub : Hub
{
    private readonly ILogger<JobFairHub> _logger;

    // Track user state in memory: ConnectionId -> AvatarState
    private static readonly ConcurrentDictionary<string, AvatarState> _avatars = new();

    public JobFairHub(ILogger<JobFairHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (_avatars.TryRemove(Context.ConnectionId, out var avatar))
        {
            await Clients.Group(avatar.FairId).SendAsync("AvatarLeft", Context.ConnectionId);
            _logger.LogInformation($"Avatar {avatar.Name} left fair {avatar.FairId}.");
        }
        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinFair(string fairId, string name, string role, string userId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, fairId);

        var avatar = new AvatarState
        {
            ConnectionId = Context.ConnectionId,
            FairId = fairId,
            Name = name,
            Role = role,
            UserId = userId,
            X = role == "Employer" ? 100 : 400, // Employers spawn near booths, candidates elsewhere
            Y = role == "Employer" ? 100 : 400
        };

        _avatars[Context.ConnectionId] = avatar;

        // Tell everyone else this person joined
        await Clients.Group(fairId).SendAsync("AvatarJoined", avatar);

        // Tell the new person about everyone already in the room
        var existingAvatars = _avatars.Values.Where(a => a.FairId == fairId).ToList();
        await Clients.Caller.SendAsync("SyncAvatars", existingAvatars);
    }

    public async Task UpdatePosition(string fairId, double x, double y)
    {
        if (_avatars.TryGetValue(Context.ConnectionId, out var avatar))
        {
            avatar.X = x;
            avatar.Y = y;
            // Broadcast to everyone EXCEPT the sender to prevent rubber-banding
            await Clients.GroupExcept(fairId, Context.ConnectionId).SendAsync("AvatarMoved", Context.ConnectionId, x, y);
        }
    }
}

public class AvatarState
{
    public string ConnectionId { get; set; } = string.Empty;
    public string FairId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public double X { get; set; }
    public double Y { get; set; }
}
