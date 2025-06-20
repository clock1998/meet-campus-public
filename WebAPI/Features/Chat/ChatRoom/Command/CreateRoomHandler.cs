using System.Data;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
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

        public async Task<Room> HandleAsync(CreateRoomRequest request, ApplicationUser currentUser)
        {
            var room = new Room();
            var users = await _context.Users.Where(n => request.UserIds.Distinct().Contains(n.Id)).ToListAsync();
            foreach (var user in users)
            {
                if (user == null) throw new NoNullAllowedException("User is null");
                room.ApplicationUsers.Add(user);
            }

            if (users.All(n => n.Id == currentUser.Id))
            {
                room.Name = currentUser.UserName!;    
            }
            else
            {
                room.Name = String.Concat(users.Where(n=> n.Id != currentUser.Id).Select(n => n.UserName));    
            }
            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();

            return room;
        }
    }
}
