using System.Data;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using WebAPI.Features.Auth;
using WebAPI.Infrastructure.Context;

namespace WebAPI.Features.Chat.ChatRoom.Command
{
    public sealed record CreateRoomRequest(List<Guid> UserIds);
    public sealed record CreateRoomResponse(Guid Id, string Content, string Username);
    public class CreateRoomHandler
    {
        private readonly AppDbContext _context;
        public CreateRoomHandler(AppDbContext context) 
        { 
            _context = context;
        } 

        public async Task<Room> HandleAsync(CreateRoomRequest request)
        {
            var room = new Room();
            foreach (var userId in request.UserIds.Distinct())
            {
                var user = await _context.Users.FindAsync(userId);
                if (user != null) room.ApplicationUsers.Add(user);
                throw new NoNullAllowedException("User is null");
            }
            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();

            return room;
        }
    }
}
