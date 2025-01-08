using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Infrastructure.Context;

namespace WebAPI.Features.Semesters.Command
{
    public sealed record CreateSemesterRequest(int Year, string Name);
    public sealed record CreateSemesterResponse(Guid Id, int Year, string Name);
    public class CreateSemesterHandler : Handler
    {
        public CreateSemesterHandler(AppDbContext context) : base(context) { }

        public async Task<CreateSemesterResponse> HandleAsync(CreateSemesterRequest request)
        {
            var obj = new Semester
            {
                Year = request.Year,
                Name = request.Name
            };

            _context.Semesters.Add(obj);
            await _context.SaveChangesAsync();

            return new CreateSemesterResponse(obj.Id, obj.Year, obj.Name);
        }
    }

    public class CreateSemesterController : SemesterController
    {
        private readonly CreateSemesterHandler _handler;
        public CreateSemesterController(AppDbContext context)
        {
            _handler = new CreateSemesterHandler(context);
        }
        [HttpPost]
        [SwaggerOperation(Tags = new[] { "Semester" })]
        public async Task<IActionResult> Create([FromBody] CreateSemesterRequest request)
        {
            var result = await _handler.HandleAsync(request);
            return Ok(result);
        }
    }
}
