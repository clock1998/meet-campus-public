using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Features.CMS.Emails.Command;
using WebAPI.Infrastructure.Context;

namespace WebAPI.Features.Chat.ChatMessage.Command
{
    public sealed record CreateMessageRequest(Guid UserId, Guid RoomId, string Content);
    public sealed record CreateMessageResponse(Guid Id, string Content, string Username, DateTime Created = default, DateTime Updated = default);
    public class CreateMessagesHandler(AppDbContext context)
    {
        public async Task<CreateMessageResponse> HandleAsync(CreateMessageRequest request)
        {
            var message = new Message
            {
                Content = request.Content,
                ApplicationUserId= request.UserId,
                RoomId = request.RoomId,
            };

            context.Messages.Add(message);
            await context.SaveChangesAsync();

            return new CreateMessageResponse(message.Id, message.Content, message.ApplicationUser?.UserName!, message.Created, message.Updated);
        }
    }
}
