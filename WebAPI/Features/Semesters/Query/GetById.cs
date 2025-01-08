using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Infrastructure.Context;

namespace WebAPI.Features.Semesters.Query
{
    public record GetSemesterByIdResponse(Guid Id, int Year, string Name);
    public class GetSemesterByIdHandler : Handler
    {
        public GetSemesterByIdHandler(AppDbContext context) : base(context) { }

        public async Task<GetSemesterByIdResponse?> HandleAsync(Guid id)
        {
            var entity = await _context.Semesters.FindAsync(id);
            if (entity is null) 
            { 
                return null;
            }
            return new GetSemesterByIdResponse(entity.Id, entity.Year, entity.Name);
        }
    }
    
    public class GetSemesterByIdController : SemesterController
    {
        private readonly GetSemesterByIdHandler _handler;
        public GetSemesterByIdController(AppDbContext context)
        {
            _handler = new GetSemesterByIdHandler(context);
        }

        [HttpGet("{id:Guid}")]
        [SwaggerOperation(Tags = new[] { "Semester" })]
        public async Task<IActionResult> Get(Guid id)
        {
            var result = await _handler.HandleAsync(id);
            if (result is null)
            {
                return NotFound();
            }
            return Ok(result);
        }
    }
}
