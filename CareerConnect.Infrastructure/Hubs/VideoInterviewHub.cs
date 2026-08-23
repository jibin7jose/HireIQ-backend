using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace CareerConnect.Infrastructure.Hubs;

[Authorize]
public class VideoInterviewHub : Hub
{
    private readonly ILogger<VideoInterviewHub> _logger;

    public VideoInterviewHub(ILogger<VideoInterviewHub> logger)
    {
        _logger = logger;
    }

    public async Task JoinRoom(string roomId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
        // Notify others in the room that someone joined
        await Clients.Group(roomId).SendAsync("UserJoined", Context.ConnectionId);
        _logger.LogInformation("User {ConnectionId} joined video room {RoomId}", Context.ConnectionId, roomId);
    }

    public async Task LeaveRoom(string roomId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
        await Clients.Group(roomId).SendAsync("UserLeft", Context.ConnectionId);
        _logger.LogInformation("User {ConnectionId} left video room {RoomId}", Context.ConnectionId, roomId);
    }

    public async Task SendOffer(string roomId, object offer)
    {
        // Forward offer to others in the room
        await Clients.OthersInGroup(roomId).SendAsync("ReceiveOffer", offer, Context.ConnectionId);
    }

    public async Task SendAnswer(string roomId, object answer)
    {
        // Forward answer to others in the room
        await Clients.OthersInGroup(roomId).SendAsync("ReceiveAnswer", answer, Context.ConnectionId);
    }

    public async Task SendIceCandidate(string roomId, object candidate)
    {
        // Forward ICE candidate to others in the room
        await Clients.OthersInGroup(roomId).SendAsync("ReceiveIceCandidate", candidate, Context.ConnectionId);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // We could track which rooms a user is in and notify them, but standard practice is 
        // they send LeaveRoom explicitly or the frontend handles disconnects gracefully.
        await base.OnDisconnectedAsync(exception);
    }
}
