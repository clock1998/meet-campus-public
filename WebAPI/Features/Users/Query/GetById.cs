using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Features.Auth;
using WebAPI.Features.Semesters.Command;
using WebAPI.Infrastructure.Context;

namespace WebAPI.Features.Users.Query
{
    public class GetUserByIdHandler : Handler
    {
        public sealed record UserResponse(Guid Id, string? Email, byte[]? Avatar, bool EmailConfirmed, UserProfile UserProfile, List<string> UserRoles);
        public GetUserByIdHandler(AppDbContext context) : base(context)
        {

        }
        public async Task<UserResponse> HandleAsync(string id)
        {
            var model = await _context.Users
                .Select(n => new UserResponse(n.Id, n.Email, n.Avatar, n.EmailConfirmed, n.UserProfile, n.UserRoles.Select(x => x.Role.Name!).ToList()))
                .FirstOrDefaultAsync(n => n.Id == new Guid(id));
            if (model == null)
            {
                throw new Exception("User not found");
            }
            return model;
        }
    }
    public class GetByIdController : UserController
    {
        private readonly GetUserByIdHandler _handler;
        public GetByIdController(GetUserByIdHandler handler)
        {
            _handler = handler;
        }

        [SwaggerOperation(Tags = new[] { "User" })]
        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> Get([FromRoute] string id)
        {
            return Ok(await _handler.HandleAsync(id));
        }
    }
}
