using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Infrastructure;
using WebAPI.Infrastructure.Context;
using WebAPI.Infrastructure.Helper;

namespace WebAPI.Features.Chat.ChatMessage.Query
{
    public sealed record GetAllByRoomIdHandlerMessageResponse(Guid Id, string Content, DateTime Created, [property: JsonPropertyName("username")] string? Username);
    public class GetMessagesByRoomIdHandler
    {
        private readonly AppDbContext _context;
        public GetMessagesByRoomIdHandler(AppDbContext context) 
        {
            _context = context;
        }

        public async Task<PagedList<GetAllByRoomIdHandlerMessageResponse>> HandleAsync(Guid roomId, QueryStringParameters queryStringParameters, Guid userId)
        {
            var user = await _context.Users.FindAsync(userId);
            var room = await _context.Rooms.FindAsync(roomId);
            if (user is null || room is null) throw new Exception("Room or user not found.");
            if (!room.ApplicationUsers.Contains(user))
            {
                throw new Exception("User is not in room.");   
            }
                
            IQueryable<Message> query = _context.Messages;

            query = query.DefaultSort(n=>n.Created, queryStringParameters.SortOrder);

            var responseQuery = query.Where(n=>n.RoomId == roomId).Select(n => new GetAllByRoomIdHandlerMessageResponse(n.Id, n.Content, n.Created, n.ApplicationUser!.UserName));

            var data = await PagedList<GetAllByRoomIdHandlerMessageResponse>.ToPagedListAsync(responseQuery, queryStringParameters.Page, queryStringParameters.PageSize);

            return data;
        }
    }
}
