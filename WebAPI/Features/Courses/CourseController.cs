using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Features.Courses
{
    [Authorize(Roles = "Admin")]
    [ApiController, Route("api/course")]
    public class CourseController : ControllerBase
    {
    }
}
