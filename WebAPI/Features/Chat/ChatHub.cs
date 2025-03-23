using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Template.WebAPI.Hubs;
using WebAPI.Features.Chat.ChatMessage.Command;
using WebAPI.Features.Chat.ChatRoom;
using WebAPI.Features.Chat.ChatRoom.Command;
using WebAPI.Features.Chat.ChatRoom.Query;
using WebAPI.Infrastructure.Context;

namespace WebAPI.Features.Chat
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly AppDbContext _context;
        private readonly CreateMessagesHandler _createMessageHandler;
        private readonly DeleteMessagesHandler _deleteMessagesHandler;
        private readonly CreateRoomHandler _createRoomHandler;
        private readonly DeleteRoomHandler _deleteRoomHandler;

        public static int ActiveUsers { get; set; } = 0;
        public static string Room { get; set; } = "";
        public ChatHub(AppDbContext context, CreateMessagesHandler createMessageHandler, DeleteMessagesHandler deleteMessagesHandler, CreateRoomHandler createRoomHandler, DeleteRoomHandler deleteRoomHandler)
        {
            _context = context;
            _createMessageHandler = createMessageHandler;
            _deleteMessagesHandler = deleteMessagesHandler;
            _createRoomHandler = createRoomHandler;
            _deleteRoomHandler = deleteRoomHandler;
        }

        public override Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                var user = _context.Users.FirstOrDefault(n => n.Id == Guid.Parse(userId));
                if (user != null)
                {
                    var username = user.UserName;
                    Clients.Users(ChatHubConnections.GetOnlineUsers()).SendAsync("ReceiveUserConnected", userId, username);
                    ChatHubConnections.AddUserConnection(userId, Context.ConnectionId);
                }
            }
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId) && ChatHubConnections.HasUserConnection(userId, Context.ConnectionId))
            {
                //Remove Disconnected user session.
                var userConnections = ChatHubConnections.GetOnlineUserSessions(userId);
                userConnections.Remove(Context.ConnectionId);
                //Notify other users 
                var userName = _context.Users.FirstOrDefault(n => n.Id == Guid.Parse(userId))?.UserName;
                Clients.Users(ChatHubConnections.GetOnlineUsers()).SendAsync("ReceiveUserDisconnected", userId, userName, ChatHubConnections.HasUser(userId));
            }
            return base.OnDisconnectedAsync(exception);
        }

        //when a room is selected, we call add to room
        public async Task CreateRoom(CreateRoomRequest request)
        {
            var room = await _createRoomHandler.HandleAsync(request);
            await Groups.AddToGroupAsync(Context.ConnectionId, room.Id.ToString());
            await Clients.Group(room.Id.ToString()).SendAsync("AddToRoom", $"{Context.ConnectionId} has joined the group {room.Id}.");
        }
        public async Task AddToRoom(string roomId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
            await Clients.Group(roomId).SendAsync("AddToRoom", $"{Context.ConnectionId} has joined the group {roomId}.");
        }

        public async Task SendMessageToRoom(CreateMessageRequest request)
        {
            await Clients.Group(request.RoomId.ToString()).SendAsync("MessageReceived", request);
            await _createMessageHandler.HandleAsync(request);
        }
        public record DeleteRoomRequest(Guid RoomId, Guid UserId);
        public async Task DeleteRoom(DeleteRoomRequest request)
        {
            var room = await _context.Rooms.FindAsync(request.RoomId);
            if (room != null)
            {
                if (room.ApplicationUsers.Any())
                {
                    room.ApplicationUsers.Remove(new Auth.ApplicationUser { Id = request.UserId });
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, request.RoomId.ToString());
                    await Clients.Group(request.RoomId.ToString()).SendAsync("Send", $"{Context.ConnectionId} has left the group {request.RoomId.ToString()}.");
                }
                else
                {
                    _context.Rooms.Remove(room);
                }
                await _context.SaveChangesAsync(); 
            }
        }
    }
}
