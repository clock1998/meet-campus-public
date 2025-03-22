using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Infrastructure;
using WebAPI.Infrastructure.Context;
using WebAPI.Infrastructure.Helper;

namespace WebAPI.Features.Chat.ChatMessage.Query
{
    public sealed record GetAllByRoomIdHandlerMessageResponse(Guid Id, string Content, DateTime Created, string? UserName);
    public sealed record GetAllByRoomIdHandlerMessageRequest(Guid RoomId);
    public class GetMessagesByRoomIdHandler
    {
        private readonly AppDbContext _context;
        public GetMessagesByRoomIdHandler(AppDbContext context) 
        {
            _context = context;
        }

        public async Task<PagedList<GetAllByRoomIdHandlerMessageResponse>> HandleAsync(GetAllByRoomIdHandlerMessageRequest request, QueryStringParameters queryStringParameters)
        {
            IQueryable<Message> query = _context.Messages;

            query = query.DefaultSort(null, queryStringParameters.SortOrder);

            var responseQuery = query.Where(n=>n.RoomId == request.RoomId).Select(n => new GetAllByRoomIdHandlerMessageResponse(n.Id, n.Content, n.Created, n.ApplicationUser!.UserName));

            var data = await PagedList<GetAllByRoomIdHandlerMessageResponse>.ToPagedListAsync(responseQuery, queryStringParameters.Page, queryStringParameters.PageSize);

            return data;
        }
    }
}
