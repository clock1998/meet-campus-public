using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Infrastructure;
using WebAPI.Infrastructure.Context;
using WebAPI.Infrastructure.Helper;

namespace WebAPI.Features.Courses.Query
{
    public class GetAllCoursesBySemesterIdHandler : Handler
    {
        public GetAllCoursesBySemesterIdHandler(AppDbContext context) : base(context) { }

        public async Task<PagedList<CourseResponse>> HandleAsync(Request request, QueryStringParameters queryStringParameters)
        {
            IQueryable<Course> query = _context.Courses;

            query = query.DefaultSort(null, queryStringParameters.SortOrder);

            var responseQuery = query.Select(n => new CourseResponse(n.Id, n.Section, n.Name, n.Semester ));

            var data = await PagedList<CourseResponse>.ToPagedListAsync(responseQuery, queryStringParameters.Page, queryStringParameters.PageSize);

            return data;
        }
    }

    public class GetAllCoursesBySemesterIdController : CourseController
    {
        private readonly GetAllCoursesBySemesterIdHandler _handler;
        public GetAllCoursesBySemesterIdController(AppDbContext context)
        {
            _handler = new GetAllCoursesBySemesterIdHandler(context);
        }

        [HttpGet("GetAllBySemesterId")]
        [SwaggerOperation(Tags = new[] { "Course" })]
        public async Task<IActionResult> Get([FromBody] Request request, [FromQuery] QueryStringParameters queryStringParameters)
        {
            return Ok(await _handler.HandleAsync(request, queryStringParameters));
        }
    }
}
