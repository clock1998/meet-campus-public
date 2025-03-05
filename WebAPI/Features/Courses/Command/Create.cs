using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Features.CMS.Emails.Command;
using WebAPI.Features.Semesters;
using WebAPI.Infrastructure.Context;

namespace WebAPI.Features.Courses.Command
{
    public sealed record CreateRequest(int Section, string Name, Guid SemesterId);
    public sealed record CreateResponse(Guid Id, int Section, string Name, Semester Semester);
    public class CreateCourseHandler : Handler
    {
        public CreateCourseHandler(AppDbContext context) : base(context) { }

        public async Task<CreateResponse> HandleAsync(CreateRequest request)
        {
            var obj = new Course
            {
                Section = request.Section,
                Name = request.Name,
                SemesterId = request.SemesterId
            };

            _context.Courses.Add(obj);
            await _context.SaveChangesAsync();

            return new CreateResponse(obj.Id, obj.Section, obj.Name, obj.Semester);
        }
    }

    public class CreateCourseController : CourseController
    {
        private readonly CreateCourseHandler _handler;
        public CreateCourseController(AppDbContext context)
        {
            _handler = new CreateCourseHandler(context);
        }
        [SwaggerOperation(Tags = new[] { "Course" })]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateRequest request)
        {
            var result = await _handler.HandleAsync(request);
            return Ok(result);
        }
    }
}
