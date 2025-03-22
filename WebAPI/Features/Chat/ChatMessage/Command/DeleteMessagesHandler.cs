using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Infrastructure.Context;

namespace WebAPI.Features.Chat.ChatMessage.Command
{
    public class DeleteMessagesHandler
    {
        protected readonly AppDbContext _context;
        public DeleteMessagesHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> HandleAsync(Guid id)
        {
            var result = await _context.Messages.FindAsync(id);
            if (result == null) return false;

            _context.Messages.Remove(result);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
