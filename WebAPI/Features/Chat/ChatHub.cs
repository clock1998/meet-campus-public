using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.RegularExpressions;
using WebAPI.Features.Auth;
using WebAPI.Features.Chat.ChatMessage;
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

        public ChatHub(AppDbContext context, CreateMessagesHandler createMessageHandler,
            DeleteMessagesHandler deleteMessagesHandler, CreateRoomHandler createRoomHandler,
            DeleteRoomHandler deleteRoomHandler)
        {
            _context = context;
            _createMessageHandler = createMessageHandler;
            _deleteMessagesHandler = deleteMessagesHandler;
            _createRoomHandler = createRoomHandler;
            _deleteRoomHandler = deleteRoomHandler;
        }
        
        private record ChatRoom(string Id, string Name, Message? LastMessage, List<Message> Messages, List<ApplicationUser> Users);
        public override Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = _context.Users.FirstOrDefault(n => userId != null && n.Id == Guid.Parse(userId));

            if (user != null)
            {
                var username = user.UserName;
                var roomIds = user.Rooms.Select(n => n.Id.ToString());
                ChatHubConnections.AddUserConnection(user, Context.ConnectionId);
                Clients.Users(ChatHubConnections.GetOnlineUsers().Select(n => n.Id.ToString()).ToList())
                .SendAsync("ConnectedUserHandler", $"{username} is connected.");
                Clients.Caller.SendAsync("OnlineUsersHandler", ChatHubConnections.GetOnlineUsers());
                
                var connectionIds = ChatHubConnections.GetOnlineUserSessions(user.Id);
                foreach (var connectionId in connectionIds)
                {
                    foreach (var roomId in roomIds)
                    {
                        Groups.AddToGroupAsync(connectionId, roomId);
                    }
                }
                Clients.Caller.SendAsync("GetChatRoomsHandler", user.Rooms.Select(n=> new ChatRoom(n.Id.ToString(), n.Name, n.Messages.LastOrDefault(), n.Messages.OrderByDescending(m => m.Created).ToList(), n.ApplicationUsers.ToList())));
            }

            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = _context.Users.FirstOrDefault(n => userId != null && n.Id == Guid.Parse(userId));
            if (user != null && ChatHubConnections.HasUserConnection(user, Context.ConnectionId))
            {
                //Remove Disconnected user session.
                var userConnections = ChatHubConnections.GetOnlineUserSessions(user);
                userConnections.Remove(Context.ConnectionId);
                Clients.Users(ChatHubConnections.GetOnlineUsers().Select(n => n.Id.ToString()).ToList())
                    .SendAsync("UserDisconnectedHandler", ChatHubConnections.GetOnlineUsers());
            }

            return base.OnDisconnectedAsync(exception);
        }

        //when a room is selected, we call add to room
        public async Task CreateRoom(CreateRoomRequest request)
        {
            var user = ChatHubConnections.GetOnlineUser(Context.ConnectionId);
            var room = await _createRoomHandler.HandleAsync(request,user);

            await Clients.Caller.SendAsync("CreateRoomHandler", room.Id);
            await Clients.Caller.SendAsync("GetChatRoomsHandler", user.Rooms.Select(n=> new ChatRoom(n.Id.ToString(), n.Name, n.Messages.LastOrDefault(), n.Messages.OrderByDescending(m => m.Created).ToList(), n.ApplicationUsers.ToList())));
            // await Clients.Group(room.Id.ToString()).SendAsync("CreateRoomHandler",
            //     $"{String.Concat(ChatHubConnections.GetOnlineUsers().FindAll(n=>request.UserIds.Contains( n.Id)).Select(n=>n.UserName), ",")} has joined the group {room.Id}.");
        }

        public async Task JoinRoom(Guid roomId)
        {
            var room = await _context.Rooms.FindAsync(roomId);
            if (room is not null)
            {
                foreach (var user in room.ApplicationUsers.Distinct())
                {
                    var connectionIds = ChatHubConnections.GetOnlineUserSessions(user.Id);
                    foreach (var connectionId in connectionIds)
                    {
                        await Groups.AddToGroupAsync(connectionId, room.Id.ToString());
                        await Clients.Group(room.Id.ToString())
                            .SendAsync("JoinRoomHandler", $"{user.UserName} has joined the group {roomId}.");
                    }
                }
            }
        }

        public async Task SendMessageToRoom(CreateMessageRequest request)
        {
            var message = await _createMessageHandler.HandleAsync(request);
            await Clients.Group(request.RoomId.ToString()).SendAsync("SendMessageToRoomHandler", message);
        }

        public record DeleteRoomRequest(Guid RoomId, Guid UserId);

        public async Task DeleteRoom(DeleteRoomRequest request)
        {
            var room = await _context.Rooms.FindAsync(request.RoomId);
            if (room == null)
            {
                await Clients.Caller.SendAsync("DeleteRoomHandler", "Room not found.");
                return;
            }
        
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId != request.UserId.ToString())
            {
                await Clients.Caller.SendAsync("DeleteRoomHandler", "Unauthorized to delete this room.");
                return;
            }
        
            if (room.ApplicationUsers.Any())
            {
                room.ApplicationUsers.Remove(new Auth.ApplicationUser { Id = request.UserId });
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, request.RoomId.ToString());
                await Clients.Group(request.RoomId.ToString()).SendAsync("DeleteRoomHandler",
                    $"{Context.ConnectionId} has left the group {request.RoomId}.");
            }
            else
            {
                _context.Rooms.Remove(room);
                await Clients.Group(request.RoomId.ToString()).SendAsync("DeleteRoomHandler",
                    $"Room {request.RoomId} has been deleted.");
            }
        
            await _context.SaveChangesAsync();
        }
    }
}