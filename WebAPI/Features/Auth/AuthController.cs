using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Features.Auth.Command;
using WebAPI.Features.Auth.Query;
using WebAPI.Features.Email;

namespace WebAPI.Features.Auth
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(
        RegisterHandler _registerHandler, IValidator<RegisterRequest> _registerValidator,
        VerifyEmailHander _verifyEmailhandler, IValidator<VerifyEmailRequest> _verifyEmailValidator,
        LoginHandler _loginHandler, IValidator<LoginRequest> _loginRequestValidator, 
        RefreshHandler _refreshHandler, IValidator<RefreshRequest> _refreshRequestValidator,
        SendVerificationEmailHandler _sendVerificationEmailHandler, ILogger<AuthController> logger
        ) : ControllerBase
    {
        [HttpPost, Route("Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var validatorResult = await _loginRequestValidator.ValidateAsync(request);
            if (!validatorResult.IsValid)
            {
                return Problem(detail: "Invalide input", instance: null, StatusCodes.Status400BadRequest, title: "Bad Request",
                     extensions: new Dictionary<string, object?>{
                        { "erros", validatorResult.Errors.Select(n => n.ErrorMessage).ToArray()}
                     });
            }
            try
            {
                return Ok(await _loginHandler.HandleAsync(request));
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, instance: null, 400, title: "Login Error", type: "Login Error");
            }
        }

        [HttpPost, Route("Register")]
        public async Task<IActionResult> Create([FromBody] RegisterRequest request)
        {
            var validatorResult = await _registerValidator.ValidateAsync(request);
            if (!validatorResult.IsValid)
            {
                return Problem(detail: "Invalide input", instance: null, StatusCodes.Status400BadRequest, title: "Bad Request",
                     extensions: new Dictionary<string, object?>{
                        { "erros", validatorResult.Errors.Select(n => n.ErrorMessage).ToArray()}
                     });
            }
            try
            {
                var user = await _registerHandler.HandleAsync(request);
                if(user is not null)
                {
                    await _sendVerificationEmailHandler.HandleAsync(user);
                }
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, instance: null, StatusCodes.Status400BadRequest, title: "Register Error", type: "Bad Request");
            }
            return Ok();
        }

        [HttpGet("VerifyEmail/{id:Guid}/{token}", Name = "VerifyEmail")]
        [SwaggerOperation(Tags = new[] { "Auth" })]
        public async Task<IActionResult> VerifyEmail([FromRoute] Guid id, [FromRoute] string token)
        {
            var request = new VerifyEmailRequest(id, token);
            var validatorResult = await _verifyEmailValidator.ValidateAsync(request);
            if (!validatorResult.IsValid)
            {
                return Problem(detail: "Invalide input", instance: null, StatusCodes.Status400BadRequest, title: "Bad Request",
                     extensions: new Dictionary<string, object?>{
                        { "erros", validatorResult.Errors.Select(n => n.ErrorMessage).ToArray()}
                     });

            }
            var result = await _verifyEmailhandler.HandleAsync(request);
            return Ok(result);
        }

        [HttpPost]
        [Route("RefreshToken")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshRequest request)
        {
            var validatorResult = await _refreshRequestValidator.ValidateAsync(request);
            if (!validatorResult.IsValid)
            {
                return Problem(detail: "Invalide input", instance: null, StatusCodes.Status400BadRequest, title: "Bad Request",
                     extensions: new Dictionary<string, object?>{
                        { "erros", validatorResult.Errors.Select(n => n.ErrorMessage).ToArray()}
                     });
            }
            try
            {
                var result = await _refreshHandler.HandlerAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                // Log the exception details
                //_logger.LogError(ex, "Error occurred while refreshing token");
                return Problem(detail: ex.Message, instance: null, statusCode: 500, title: "RefreshAsync", type: "Exception");
            }
        }
    }
}
