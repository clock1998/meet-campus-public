using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Features.CMS.Emails.Command;
using WebAPI.Infrastructure.Context;

namespace WebAPI.Features.Chat.ChatMessage.Command
{
    public sealed record CreateMessageRequest(Guid UserId, Guid RoomId, string Content);
    public sealed record CreateResponse(Guid Id, string Content, string Username);
    public class CreateMessagesHandler
    {
        protected readonly AppDbContext _context;
        public CreateMessagesHandler(AppDbContext context) 
        { 
            _context = context;
        } 

        public async Task<CreateResponse> HandleAsync(CreateMessageRequest request)
        {
            var message = new Message
            {
                Content = request.Content,
                ApplicationUserId= request.UserId,
                RoomId = request.RoomId,
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            return new CreateResponse(message.Id, message.Content, message.ApplicationUser?.UserName!);
        }
    }
}
