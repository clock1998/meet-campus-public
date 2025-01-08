using EmailService;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Features.Users;
using WebAPI.Infrastructure.Context;
using WebAPI.Infrastructure.Helper;

namespace WebAPI.Features.Auth
{
    public sealed record VerifyEmailRequest(Guid Id, string Token);
    public sealed class VerifyEmailValidator : AbstractValidator<VerifyEmailRequest>
    {
        public VerifyEmailValidator()
        {
            RuleFor(n => n.Id).NotEmpty();
            RuleFor(n => n.Token).NotEmpty();
        }
    }
    public class VerifyEmailHander
    {
        private readonly AuthHandler _authHandler;
        public VerifyEmailHander(AuthHandler authHandler)
        {
            _authHandler = authHandler;
        }

        public async Task<bool> HandleAsync(VerifyEmailRequest request)
        {
            var user = await _authHandler.UserManager.FindByIdAsync(request.Id.ToString());
            if (user == null)
            {
                throw new Exception("User not found.");
            }
            var result = await _authHandler.UserManager.ConfirmEmailAsync(user, request.Token.Base64Decode());
            return result.Succeeded;
        }
    }

    public class VerifyEmailController : AuthController
    {
        private readonly VerifyEmailHander _handler;
        private readonly IValidator<VerifyEmailRequest> _validator;
        public VerifyEmailController(VerifyEmailHander handler, IValidator<VerifyEmailRequest> validator)
        {
            _handler = handler;
            _validator = validator;
        }

        [HttpGet("VerifyEmail/{id:Guid}/{token}")]
        [SwaggerOperation(Tags = new[] { "Auth" })]
        public async Task<IActionResult> VerifyEmail([FromRoute] Guid id, [FromRoute] string token)
        {
            var request = new VerifyEmailRequest(id, token);
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
