using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Infrastructure.Context;

namespace WebAPI.Features.Semesters.Command
{
    public sealed record UpdateSemesterRequest(Guid Id, int Year, string Name);
    public class UpdateSemesterHandler : Handler
    {
        public UpdateSemesterHandler(AppDbContext context) : base(context) { }

        public async Task<bool> HandleAsync(UpdateSemesterRequest command)
        {
            var result = await _context.Semesters.FindAsync(command.Id);
            if (result == null) return false;

            result.Year = command.Year;
            result.Name = command.Name;

            await _context.SaveChangesAsync();
            return true;
        }
    }

    public class UpdateSemesterController : SemesterController
    {
        private readonly UpdateSemesterHandler _handler;
        public UpdateSemesterController(AppDbContext context)
        {
            _handler = new UpdateSemesterHandler(context);
        }
        [HttpPut]
        [SwaggerOperation(Tags = new[] { "Semester" })]
        public async Task<IActionResult> Update([FromBody] UpdateSemesterRequest request)
        {
            var result = await _handler.HandleAsync(request);
            return Ok(result);
        }
    }
}
