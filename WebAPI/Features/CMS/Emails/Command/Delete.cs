using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Infrastructure.Context;

namespace WebAPI.Features.CMS.Emails.Command
{
    public class DeleteEmailHandler : Handler
    {
        public DeleteEmailHandler(AppDbContext context) : base(context) { }

        public async Task<bool> HandleAsync(Guid id)
        {
            var result = await _context.Emails.FindAsync(id);
            if (result == null) return false;

            _context.Emails.Remove(result);
            await _context.SaveChangesAsync();
            return true;
        }
    }

    [Authorize(Roles = "Admin")]
    [ApiController, Route("api/cms/email")]
    public class DeleteController : Controller
    {
        private readonly DeleteEmailHandler _handler;
        public DeleteController(DeleteEmailHandler handler)
        {
            _handler = handler;
        }
        [HttpDelete("{id:Guid}")]
        [SwaggerOperation(Tags = new[] { "Email" })]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            var result = await _handler.HandleAsync(id);
            if(!result) return NotFound();
            return Ok(result);
        }
    }
}
