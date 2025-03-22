using WebAPI.Features.Chat.ChatMessage.Query;
using WebAPI.Infrastructure.Context;
using WebAPI.Infrastructure.Helper;
using WebAPI.Infrastructure;

namespace WebAPI.Features.Chat.ChatRoom.Query
{
    public class GetAllRoomsHandler
    {
        private readonly AppDbContext _context;
        public GetAllRoomsHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PagedList<Room>> HandleAsync(GetAllByRoomIdHandlerMessageRequest request, QueryStringParameters queryStringParameters)
        {
            IQueryable<Room> query = _context.Rooms;

            query = query.DefaultSort(null, queryStringParameters.SortOrder);

            var responseQuery = query;

            var data = await PagedList<Room>.ToPagedListAsync(responseQuery, queryStringParameters.Page, queryStringParameters.PageSize);

            return data;
        }
    }
}
