using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Reflection.Metadata;
using System.Security.Claims;
using WebAPI.Features.Chat.ChatMessage.Command;
using WebAPI.Features.Chat.ChatMessage.Query;
using WebAPI.Features.Chat.ChatRoom.Command;
using WebAPI.Features.Chat.ChatRoom.Query;
using WebAPI.Infrastructure;
using WebAPI.Infrastructure.Context;

namespace WebAPI.Features.Chat.ChatMessage
{
    [Authorize]
    [ApiController, Route("api/[controller]")]
    public class MessageController : ControllerBase
    {
        private readonly CreateMessagesHandler _createMessageHandler;
        private readonly DeleteMessagesHandler _deleteMessagesHandler;
        private readonly GetMessagesByRoomIdHandler _getMessagesByRoomIdHandler;
        public MessageController(AppDbContext context, CreateMessagesHandler createMessagesHandler, DeleteMessagesHandler deleteMessagesHandler, GetMessagesByRoomIdHandler getMessagesByRoomIdHandler)
        {
            _createMessageHandler = createMessagesHandler;
            _deleteMessagesHandler = deleteMessagesHandler;
            _getMessagesByRoomIdHandler = getMessagesByRoomIdHandler;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateMessageRequest request)
        {
            var result = await _createMessageHandler.HandleAsync(request);
            return Ok(result);
        }

        [HttpDelete("{id:Guid}")]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            var result = await _deleteMessagesHandler.HandleAsync(id);
            return Ok(result);
        }

        [HttpGet("GetByRoomId")]
        public async Task<IActionResult> GetMessagesByRoomId([FromQuery] Guid roomId, [FromQuery] QueryStringParameters queryStringParameters)
        {
            queryStringParameters.PageSize = 7;
            queryStringParameters.SortOrder = "desc";
            var userId = User.FindFirstValue( ClaimTypes.NameIdentifier);
            if(userId is not null)
                return Ok(await _getMessagesByRoomIdHandler.HandleAsync(roomId, queryStringParameters, new Guid(userId)));
            return Problem(
                detail: "User not found", 
                instance: null, 
                StatusCodes.Status400BadRequest, title: "Bad Request"
                );
        }
    }
}
