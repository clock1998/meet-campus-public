using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebAPI.Infrastructure.Context;

namespace WebAPI.Hubs
{
    //[Authorize]
    //public class MessageHub : Hub
    //{
    //    private readonly AppDbContext context;
    //    private readonly IMessageRepository messageRepository;

    //    public static int ActiveUsers { get; set; } = 0;
    //    public static string Room { get; set; } = "";
    //    public ChatHub(AppDbContext context, IMessageRepository messageRepository)
    //    {
    //        this.context = context;
    //        this.messageRepository = messageRepository;
    //    }
    //    public class HubMessage
    //    {
    //        public Guid Id { get; set; }
    //        public string Content { get; set; } = null!;
    //        public string ApplicationUserId { get; set; } = null!;
    //        public Guid RoomId { get; set; }

    //    }
    //    public async Task SendMessageToRoom(HubMessage hubMessage)
    //    {
    //        var message = await messageRepository.GetAsync(hubMessage.Id);
    //        await Clients.Group(message.RoomId.ToString()).SendAsync("MessageReceived", message);
    //    }
    //    //when a room is selected, we call add to room
    //    public async Task AddToRoom(string roomId)
    //    {
    //        await Groups.AddToGroupAsync(Context.ConnectionId, roomId);

    //        //await Clients.Group(roomId).SendAsync("AddToRoom", $"{Context.ConnectionId} has joined the group {roomId}.");
    //    }
    //    //when a room is unselected, we call remove from room
    //    public async Task RemoveFromRoom(string roomId)
    //    {
    //        await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);

    //        //await Clients.Group(roomId).SendAsync("Send", $"{Context.ConnectionId} has left the group {roomId}.");
    //    }

    //    public override Task OnConnectedAsync()
    //    {
    //        var userId = Context.User.FindFirstValue(ClaimTypes.NameIdentifier);
    //        if (!string.IsNullOrEmpty(userId))
    //        {
    //            var userName = context.Users.FirstOrDefault(n => n.Id == userId).UserName;
    //            Clients.Users(HubConnections.OnlineUsers()).SendAsync("ReceiveUserConnected", userId, userName);
    //            HubConnections.AddUserConnection(userId, Context.ConnectionId);
    //        }
    //        return base.OnConnectedAsync();
    //    }

    //    public override Task OnDisconnectedAsync(Exception? exception)
    //    {
    //        var userId = Context.User.FindFirstValue(ClaimTypes.NameIdentifier);
    //        if (HubConnections.HasUserConnection(userId, Context.ConnectionId))
    //        {
    //            var userConnections = HubConnections.Users[userId];
    //            userConnections.Remove(Context.ConnectionId);
    //            //HubConnections.Users.Remove(userId);
    //        }
    //        if (!string.IsNullOrEmpty(userId))
    //        {
    //            var userName = context.Users.FirstOrDefault(n => n.Id == userId).UserName;
    //            Clients.Users(HubConnections.OnlineUsers()).SendAsync("ReceiveUserDisconnected", userId, userName, HubConnections.HasUser(userId));
    //            HubConnections.AddUserConnection(userId, Context.ConnectionId);
    //        }
    //        return base.OnDisconnectedAsync(exception);
    //    }
    //    public async Task SendMessageToReceiver(string sender, string receiver, string message)
    //    {
    //        var userId =
    //    }
    //}
}
