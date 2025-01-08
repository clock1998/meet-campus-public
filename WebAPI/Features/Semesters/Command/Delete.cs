using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Infrastructure.Context;

namespace WebAPI.Features.Semesters.Command
{
    public class DeleteSemesterHandler : Handler
    {
        public DeleteSemesterHandler(AppDbContext context) : base(context){}

        public async Task<bool> HandleAsync(Guid id)
        {
            var result = await _context.Semesters.FindAsync(id);
            if (result == null) return false;

            _context.Semesters.Remove(result);
            await _context.SaveChangesAsync();
            return true;
        }
    }

    public class DeleteSemesterController : SemesterController
    {
        private readonly DeleteSemesterHandler _deleteSemesterHandler;
        public DeleteSemesterController(AppDbContext context)
        {
            _deleteSemesterHandler = new DeleteSemesterHandler(context);
        }

        [Route("{id:Guid}")]
        [HttpDelete]
        [SwaggerOperation(Tags = new[] { "Semester" })]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            var result = await _deleteSemesterHandler.HandleAsync(id);
            return Ok(result);
        }
    }
}
