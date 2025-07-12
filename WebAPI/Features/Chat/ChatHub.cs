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
// ReSharper disable All

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
        
        private record ChatMessage(string Id, string Content, string Username, DateTime Updated = default);
        private record ChatRoom(string Id, string Name, ChatMessage? LastMessage, List<Message>? Messages, List<ApplicationUser>? Users);
        public override async Task OnConnectedAsync()
        {
            var currentUser =  await GetCurrentUserAsync();
            var username = currentUser.UserName;
            var roomIds = currentUser.Rooms.Select(n => n.Id.ToString());
            ChatHubConnections.AddUserConnection(currentUser.Id.ToString(), Context.ConnectionId);
            // await Clients.Users(ChatHubConnections.GetOnlineUsers().Select(n => n.Id.ToString()).ToList())
            // .SendAsync("ConnectedUserHandler", $"{username} is connected.");
            // await Clients.Caller.SendAsync("OnlineUsersHandler", ChatHubConnections.GetOnlineUsers());
            var onlineUsers = 
                _context.Users
                    .Where(n=>ChatHubConnections.GetOnlineUsers().Contains(n.Id.ToString()))
                    .Select(n=>new User(n)).ToList();
            await Clients.All.SendAsync("OnlineUsersHandler", onlineUsers);
            var connectionIds = ChatHubConnections.GetOnlineUserSessions(currentUser.Id.ToString());
            foreach (var connectionId in connectionIds)
            {
                foreach (var roomId in roomIds)
                {
                    await Groups.AddToGroupAsync(connectionId, roomId);
                }
            }
            await SendChatRoomsToCallerAsync();
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var currentUser =  await GetCurrentUserAsync();
            if (ChatHubConnections.HasUserConnection(currentUser.Id.ToString(), Context.ConnectionId))
            {
                //Remove Disconnected user session.
                var userConnections = ChatHubConnections.GetOnlineUserSessions(currentUser.Id.ToString());;
                userConnections.Remove(Context.ConnectionId);
                if (!userConnections.Any())
                {
                    ChatHubConnections.RemoveUser(currentUser.Id.ToString());
                }

                var onlineUsers =
                    _context.Users
                        .Where(n => ChatHubConnections.GetOnlineUsers().Contains(n.Id.ToString()))
                        .Select(n => new User(n)).ToList();
                await Clients.All
                    .SendAsync("UserDisconnectedHandler", onlineUsers);
            }
            await SendChatRoomsToCallerAsync();
            await base.OnDisconnectedAsync(exception);
        }

        //when a room is selected, we call add to room
        public async Task CreateRoom(CreateRoomRequest request)
        {
            var currentUser =  await GetCurrentUserAsync();
            var room = await _createRoomHandler.HandleAsync(request, currentUser);
            var userIds = request.UserIds.Select(n => n.ToString()).ToList();
            await Clients.Users(userIds)
                .SendAsync("CreateRoomHandler", 
                    new ChatRoom(
                        room.Id.ToString(), 
                        room.Name, 
                        null, 
                        room.Messages.Any() ? room.Messages.OrderByDescending(m => m.Created).ToList(): null, 
                        room.ApplicationUsers.ToList()));
            // await Clients.Group(room.Id.ToString()).SendAsync("CreateRoomHandler",
            //     $"{String.Concat(ChatHubConnections.GetOnlineUsers().FindAll(n=>request.UserIds.Contains( n.Id)).Select(n=>n.UserName), ",")} has joined the group {room.Id}.");
        }


        public async Task JoinRoom(Guid roomId)
        {
            var currentConnectionId = Context.ConnectionId;
            var user = await GetCurrentUserAsync();
            var room = await _context.Rooms.FindAsync(roomId);
            if (room is not null)
            {
                var connectionIds = ChatHubConnections.GetOnlineUserSessions(user.Id.ToString()).Distinct();
                if (!connectionIds.Contains(currentConnectionId))
                {
                    await Groups.AddToGroupAsync(currentConnectionId, room.Id.ToString());
                    await Clients.Group(room.Id.ToString())
                        .SendAsync("JoinRoomHandler", $"{user.UserName} has joined the group {roomId}.");
                }
            }
        }

        public async Task SendMessageToRoom(CreateMessageRequest request)
        {
            var currentUser =  await GetCurrentUserAsync();
            var message = await _createMessageHandler.HandleAsync(request);
            await Clients.Group(request.RoomId.ToString()).SendAsync("SendMessageToRoomHandler", message);
            await Clients.Group(request.RoomId.ToString()).SendAsync("GetChatRoomsHandler", 
                currentUser.Rooms.Select(n=> 
                    new ChatRoom(
                        n.Id.ToString(), 
                        n.Name, 
                        new ChatMessage(message.Id.ToString(), message.Content, message.Username, message.Updated), 
                        n.Messages.OrderByDescending(m => m.Created).ToList(), 
                        n.ApplicationUsers.ToList())));
        }
        
        public async Task DeleteRoom(Guid roomId)
        {
            var room = await _context.Rooms.FindAsync(roomId);
            if (room is null)
            {
                await Clients.Caller.SendAsync("DeleteRoomHandler", "Room not found.");
                return;
            }
            var currentUser =  await GetCurrentUserAsync();
            if (!currentUser.Rooms.Select(n => n.Id).Contains(roomId))
            {
                await Clients.Caller.SendAsync("DeleteRoomHandler", "You are not a member of this room.");
                return;
            }
            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();
            var sessions = ChatHubConnections.GetOnlineUserSessions(currentUser.Id.ToString());
            foreach (var session in sessions)
            {
                await Groups.RemoveFromGroupAsync(session, room.Id.ToString());    
            }

            await SendChatRoomsToCallerAsync();
            // await Clients.Group(request.RoomId.ToString()).SendAsync("DeleteRoomHandler",
            //     $"Room {request.RoomId} has been deleted.");
        }
        
        private async Task SendChatRoomsToCallerAsync()
        {
            var currentUser =  await GetCurrentUserAsync();
            var test = currentUser.Rooms.Select(n =>
                new ChatRoom(
                    n.Id.ToString(),
                    n.Name,
                    n.Messages.Any()
                        ? new ChatMessage(n.Messages.LastOrDefault().Id.ToString(), n.Messages.LastOrDefault().Content,
                            n.Messages.LastOrDefault().ApplicationUser.UserName)
                        : null,
                    n.Messages.Any() ? n.Messages.OrderByDescending(m => m.Created).ToList() : null,
                    n.ApplicationUsers.ToList()));
            await Clients.Caller.SendAsync("GetChatRoomsHandler", 
                currentUser.Rooms.Select(n=> 
                    new ChatRoom(
                        n.Id.ToString(), 
                        n.Name, 
                        n.Messages.Any() ? new ChatMessage(n.Messages.LastOrDefault().Id.ToString(), n.Messages.LastOrDefault().Content, n.Messages.LastOrDefault().ApplicationUser.UserName) : null , 
                        n.Messages.Any() ? n.Messages.OrderByDescending(m => m.Created).ToList():null, 
                        n.ApplicationUsers.ToList())));
        }

        private async Task<ApplicationUser> GetCurrentUserAsync()
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                throw new Exception("User ID not found");
        
            var user = await _context.Users
                .FirstOrDefaultAsync(n => n.Id == Guid.Parse(userId));
        
            if (user == null) 
                throw new Exception("User not found");
        
            return user;
        }

    }
}