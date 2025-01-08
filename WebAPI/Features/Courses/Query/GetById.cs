using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Infrastructure;
using WebAPI.Infrastructure.Context;

namespace WebAPI.Features.Courses.Query
{
    public record GetSemesterByIdResponse(Guid Id, int Section, string Name);
    public class GetSemesterByIdHandler : Handler
    {
        public GetSemesterByIdHandler(AppDbContext context) : base(context) { }

        public async Task<GetSemesterByIdResponse> HandleAsync(Guid id)
        {
            var entity = await _context.Courses.FindAsync(id);
            if(entity == null)
            {
                throw new Exception("Course not found.");
            }
            return new GetSemesterByIdResponse(entity.Id, entity.Section, entity.Name);
        }
    }

    public class GetCoursesByIdController : CourseController
    {
        private readonly GetSemesterByIdHandler _handler;
        public GetCoursesByIdController(AppDbContext context)
        {
            _handler = new GetSemesterByIdHandler(context);
        }

        [HttpGet("{id:Guid}")]
        [SwaggerOperation(Tags = new[] { "Course" })]
        public async Task<IActionResult> Get([FromQuery] Guid id)
        {
            return Ok(await _handler.HandleAsync(id));
        }
    }
}
