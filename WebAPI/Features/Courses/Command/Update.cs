using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Features.Semesters;
using WebAPI.Infrastructure.Context;

namespace WebAPI.Features.Courses.Command
{
    public sealed record UpdateRequest(Guid Id, int Section, string Name, Guid SemesterId);
    public sealed record UpdateResponse(Guid Id, int Section, string Name, Semester Semester);
    public class UpdateCourseHandler : Handler
    {
        public UpdateCourseHandler(AppDbContext context) : base(context) { }

        public async Task<UpdateResponse> HandleAsync(UpdateRequest request)
        {
            var result = await _context.Courses.FindAsync(request.Id);
            if (result != null)
            {
                result.Name = request.Name;
                result.Section = request.Section;
                result.SemesterId = request.SemesterId;

                await _context.SaveChangesAsync();

                return new UpdateResponse(result.Id, result.Section, result.Name, result.Semester);
            }
            throw new Exception("Course not found.");
        }
    }
    public sealed class Validator : AbstractValidator<UpdateRequest>
    {
        public Validator() 
        {
            RuleFor(n => n.Id).NotEmpty();
            RuleFor(n => n.Name).NotEmpty();
            RuleFor(n => n.SemesterId).NotEmpty();
        }
    }

    public class UpdateCourseController : CourseController
    {
        private readonly UpdateCourseHandler _handler;
        private readonly IValidator<UpdateRequest> _validator;
        public UpdateCourseController(AppDbContext context, IValidator<UpdateRequest> validator)
        {
            _handler = new UpdateCourseHandler(context);
            _validator = validator;
        }

        [HttpPut("{id:Guid}")]
        [SwaggerOperation(Tags = new[] { "Course" })]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateRequest request)
        {
            var validatorResult = await _validator.ValidateAsync(request);
            if (!validatorResult.IsValid)
            {
                return Problem(detail: "Invalide input", instance: null, StatusCodes.Status400BadRequest, title: "Bad Request",
                     extensions: new Dictionary<string, object?>{
                        { "erros", validatorResult.Errors.Select(n => n.ErrorMessage).ToArray()}
                     });
            }

            var result = await _handler.HandleAsync(request);
            return Ok(result);
        }
    }
}
