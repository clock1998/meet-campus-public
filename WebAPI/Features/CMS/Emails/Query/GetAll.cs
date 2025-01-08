using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Infrastructure.Context;

namespace WebAPI.Features.CMS.Emails.Query
{
    public record Response(Guid Id, string EmailAddress);
    public class GetAllEmailsHandler : Handler
    {
        public GetAllEmailsHandler(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<Response>> HandleAsync()
        {
            var entities = await _context.Emails.Select(n => new Response(n.Id, n.EmailAdress)).ToListAsync();
            return entities;
        }
    }


    [ApiController, Route("api/email")]
    public class GetAllController : ControllerBase
    {
        private readonly GetAllEmailsHandler _handler;
        public GetAllController(GetAllEmailsHandler handler)
        {
            _handler = handler;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        [SwaggerOperation(Tags = new[] { "Email" })]
        public async Task<IActionResult> Get()
        {
            return Ok(await _handler.HandleAsync());
        }

        [HttpGet]
        [Route(nameof(GetCount))]
        [SwaggerOperation(Tags = new[] { "Email" })]
        public async Task<IActionResult> GetCount()
        {
            var result  = await _handler.HandleAsync();
            return Ok(result.Count());
        }
    }
}
