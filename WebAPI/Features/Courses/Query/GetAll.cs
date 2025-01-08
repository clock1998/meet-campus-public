using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Features.Semesters;
using WebAPI.Infrastructure.Context;

namespace WebAPI.Features.Courses.Query
{
    public record CourseResponse(Guid Id, int Section, string Name, Semester Semester);
    public class GetAllCoursesHandler : Handler
    {
        public GetAllCoursesHandler(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<CourseResponse>> HandleAsync()
        {
            var entities = await _context.Courses.Select(n => new CourseResponse(n.Id, n.Section, n.Name, n.Semester)).ToListAsync();
            return entities;
        }
    }

    public class GetAllCoursesController : CourseController
    {
        private readonly GetAllCoursesHandler _handler;
        public GetAllCoursesController(AppDbContext context)
        {
            _handler = new GetAllCoursesHandler(context);
        }

        [HttpGet]
        [SwaggerOperation(Tags = new[] { "Course" })]
        public async Task<IActionResult> Get()
        {
            return Ok(await _handler.HandleAsync());
        }
    }
}
