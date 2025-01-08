using EmailService;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Reflection.Metadata;
using Template.WebAPI.Repositories;
using WebAPI.Features.Users;
using WebAPI.Infrastructure.Context;
using WebAPI.Infrastructure.Helper;

namespace WebAPI.Features.Auth
{
    public sealed record LoginRequest(string Username, string Password);

    public sealed class LoginValidator : AbstractValidator<LoginRequest>
    {
        public LoginValidator()
        {
            RuleFor(n => n.Username).NotEmpty().EmailAddress();
            RuleFor(n => n.Password).NotEmpty();
        }
    }

    public class LoginHandler 
    {
        private readonly AuthHandler _authHandler;
        public LoginHandler( AuthHandler authHandler)
        {
            _authHandler = authHandler;
        }

        public async Task<AppContextResponse> HandleAsync(LoginRequest request)
        {
            var user = await _authHandler.UserManager.FindByEmailAsync(request.Username);
            if (user != null)
            {
                var result = await _authHandler.UserManager.CheckPasswordAsync(user, request.Password);
                var isEmailConfirmed = await _authHandler.UserManager.IsEmailConfirmedAsync(user);
                if (result)
                {
                    if (isEmailConfirmed)
                    {
                        user.RefreshToken = AuthHelper.CreateRefreshToken();
                        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddHours(24);
                        await _authHandler.UserManager.UpdateAsync(user);
                        var roles = await _authHandler.UserManager.GetRolesAsync(user);
                        if (roles.Any())
                        {
                            var response = new AppContextResponse
                            {
                                Token = AuthHelper.CreateToken(user, roles.ToList(), _authHandler.Configuration),
                                RefreshToken = user.RefreshToken,
                                User = new UserReponse
                                {
                                    Id = user.Id.ToString(),
                                    Email = user.Email!,
                                    FirstName = user.UserProfile.FirstName,
                                    LastName = user.UserProfile.LastName,
                                    Roles = user.UserRoles.Where(n => n.Role.Name != null).Select(n => n.Role.Name!).ToList(),
                                },
                            };
                            return response;
                        }
                        throw new InvalidOperationException("The user does not have a role.");
                    }
                    throw new InvalidOperationException("The user's email is not confirmed.");
                }
            }
            throw new InvalidOperationException("Username or password incorrect.");
        }
    }
    public class LoginController : AuthController
    {
        private readonly LoginHandler _handler;
        private readonly IValidator<LoginRequest> _validator;
        public LoginController(LoginHandler handler, IValidator<LoginRequest> validator, ILogger<AuthController> logger)
        {
            _handler = handler;
            _validator = validator;
        }

        [HttpPost, Route("Login")]
        [SwaggerOperation(Tags = new[] { "Auth" })]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var validatorResult = await _validator.ValidateAsync(request);
            if (!validatorResult.IsValid)
            {
                return Problem(detail: "Invalide input", instance: null, StatusCodes.Status400BadRequest, title: "Bad Request",
                     extensions: new Dictionary<string, object?>{
                        { "erros", validatorResult.Errors.Select(n => n.ErrorMessage).ToArray()}
                     });
            }
            try
            {
                return Ok(await _handler.HandleAsync(request));
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, instance: null, 400, title: "Login Error", type: "Login Error");
            }
        }
    }
}
