
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Reflection.Metadata;
using WebAPI.Features.Chat.ChatMessage.Command;
using WebAPI.Features.Chat.ChatMessage.Query;
using WebAPI.Features.Chat.ChatRoom.Command;
using WebAPI.Features.Chat.ChatRoom.Query;
using WebAPI.Infrastructure;
using WebAPI.Infrastructure.Context;

namespace WebAPI.Features.Chat.ChatRoom
{
    [Authorize]
    [ApiController, Route("api/[controller]")]
    public class ChatRoomController : ControllerBase
    {
        private readonly GetAllRoomsHandler _getAllRoomsHandler;
        public ChatRoomController(AppDbContext context, GetAllRoomsHandler getAllRoomsHandler)
        {
            _getAllRoomsHandler = getAllRoomsHandler;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] QueryStringParameters queryStringParameters)
        {
            return Ok(await _getAllRoomsHandler.HandleAsync(queryStringParameters));
        }
    }
}
