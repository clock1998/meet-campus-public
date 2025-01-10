using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Features.Auth;
using WebAPI.Infrastructure.Context;

namespace WebAPI.Features.Users.Query
{
    public class GetAllController : UserController
    {
        private readonly AppDbContext _context;
        public GetAllController(AppDbContext context)
        {
            _context = context;
        }

        [SwaggerOperation(Tags = new[] { "User" })]
        [EnableQuery]
        [HttpGet]
        public ActionResult<IQueryable<ApplicationUser>> GetAll()
        {
            return Ok(_context.Users);
        }
    }
}
