using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Infrastructure.Context;

namespace WebAPI.Features.Users.Command
{
    public class DeleteUserHandler : Handler
    {
        public DeleteUserHandler(AppDbContext context) : base(context) { }

        public async Task<bool> HandleAsync(Guid id)
        {
            var result = await _context.Users.FindAsync(id);
            if (result == null) return false;

            _context.Users.Remove(result);
            await _context.SaveChangesAsync();
            return true;
        }
    }

    public class DeleteUserController : UserController
    {
        private readonly DeleteUserHandler _handler;
        public DeleteUserController(DeleteUserHandler handler)
        {
            _handler = handler;
        }

        [SwaggerOperation(Tags = new[] { "User" })]
        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> Delete([FromRoute] string id)
        {
            return Ok(await _handler.HandleAsync(new Guid(id)));
        }
    }
}
