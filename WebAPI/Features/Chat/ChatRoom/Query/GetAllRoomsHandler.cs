using WebAPI.Features.Chat.ChatMessage.Query;
using WebAPI.Infrastructure.Context;
using WebAPI.Infrastructure.Helper;
using WebAPI.Infrastructure;

namespace WebAPI.Features.Chat.ChatRoom.Query
{
    public class GetAllRoomsHandler(AppDbContext context)
    {
        
        public async Task<PagedList<Room>> HandleAsync(QueryStringParameters queryStringParameters)
        {
            IQueryable<Room> query = context.Rooms;

            query = query.DefaultSort(null, queryStringParameters.SortOrder);

            var responseQuery = query;

            var data = await PagedList<Room>.ToPagedListAsync(responseQuery, queryStringParameters.Page, queryStringParameters.PageSize);

            return data;
        }
    }
}
