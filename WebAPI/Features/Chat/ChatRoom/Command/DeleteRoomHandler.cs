using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Infrastructure.Context;

namespace WebAPI.Features.Chat.ChatRoom.Command
{
    public class DeleteRoomHandler
    {
        protected readonly AppDbContext _context;
        public DeleteRoomHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> HandleAsync(Guid id)
        {
            var result = await _context.Rooms.FindAsync(id);
            if (result == null) return false;

            _context.Rooms.Remove(result);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
