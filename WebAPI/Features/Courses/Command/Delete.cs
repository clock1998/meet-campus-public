using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Infrastructure.Context;

namespace WebAPI.Features.Courses.Command
{
    public class DeleteCourseHandler : Handler
    {
        public DeleteCourseHandler(AppDbContext context) : base(context) { }

        public async Task<bool> HandleAsync(Guid id)
        {
            var result = await _context.Courses.FindAsync(id);
            if (result == null) return false;

            _context.Courses.Remove(result);
            await _context.SaveChangesAsync();
            return true;
        }
    }

    public class DeleteCourseController : CourseController
    {
        private readonly DeleteCourseHandler _handler;
        public DeleteCourseController(AppDbContext context)
        {
            _handler = new DeleteCourseHandler(context);
        }
        [HttpDelete("{id:Guid}")]
        [SwaggerOperation(Tags = new[] { "Course" })]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            var result = await _handler.HandleAsync(id);
            return CreatedAtAction("DeleteCourse", result);
        }
    }
}
