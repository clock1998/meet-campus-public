using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.Features.Users;
using WebAPI.Infrastructure;
using WebAPI.Infrastructure.Context;

namespace Template.WebAPI.Controllers
{
    [Route("api/[controller]"), ApiController]
    [Authorize(Roles="Admin")]
    public class UserController : Controller
    {
        private readonly AppDbContext context;
        private readonly UserManager<ApplicationUser> userManager;

        public UserController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            this.context = context;
            this.userManager = userManager;
        }

        private class UserModel
        {   
            public required Guid Id { get; set; }
            public required string Email { get; set; }
            public required bool EmailConfirmed { get; set; }
            public required string? UserProfileDisplayName { get; set; }
            
            public required UserProfile? UserProfile { get; set; }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] QueryStringParameters queryParameters)
        {
            var query = userManager.Users.AsQueryable();
            if (!string.IsNullOrEmpty(queryParameters.Search))
            {
                query = query.Where(n => n.Email!.Contains(queryParameters.Search) || n.UserProfile.FirstName.Contains(queryParameters.Search) || n.UserProfile.LastName.Contains(queryParameters.Search));
            }
            var selectedData = query.Select(n => new UserModel { Id=n.Id, Email = n.Email!, EmailConfirmed = n.EmailConfirmed, UserProfileDisplayName= n.UserProfile.DisplayName, UserProfile = n.UserProfile });

            var data = await PagedList<UserModel>.ToPagedListAsync(selectedData,
                    queryParameters.Page,
                    queryParameters.PageSize);
            return Ok(data);
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> Get([FromRoute] string id)
        {
            var model = await userManager.Users.Select(n => new { n.Id, n.Email, n.Avatar, n.EmailConfirmed, n.UserProfile, UserRoles=n.UserRoles.Select(x=>x.Role.Name)}).FirstOrDefaultAsync(n => n.Id == new Guid(id));
            if (model == null)
            {
                return NotFound();
            }
            return Ok(model);
        }

        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> Delete([FromRoute] string id)
        {
            var model = await userManager.FindByIdAsync(id);
            if (model == null)
            {
                return NotFound();
            }
            await userManager.DeleteAsync(model);
            return Ok(model);
        }

        [HttpPut]
        public async Task<IActionResult> ChangePassword([FromRoute] string id)
        {
            return Ok();
        }
    }
}
