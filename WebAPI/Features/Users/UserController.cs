using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Features.Users
{
    [Route("api/user"), ApiController]
    [Authorize(Roles = "Admin")]
    public abstract class UserController : ControllerBase
    {
        [HttpPut]
        public async Task<IActionResult> ChangePassword([FromRoute] string id)
        {
            return Ok();
        }
    }
}
