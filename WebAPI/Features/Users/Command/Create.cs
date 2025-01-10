using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Features.Auth;
using WebAPI.Infrastructure.Helper;

namespace WebAPI.Features.Users.Command
{
    public sealed record CreateUserRequest(string Email, string Password, string PasswordConfirm, string FirstName, string LastName);
    public sealed record CreateUserResponse(Guid Id, int Year, string Name);
    public class CreateUserHandler
    {
        private readonly AuthHandler _authHandler;
        public CreateUserHandler(AuthHandler authHandler)
        {
            _authHandler = authHandler;
        }

        public async Task<ApplicationUser> HandleAsync(CreateUserRequest request, IUrlHelper url, string protocol)
        {
            var email = request.Email.ToLower().Trim();
            var domain = email.Trim().Split("@")[1];

            var existingUser = await _authHandler.UserManager.FindByEmailAsync(request.Email);
            //if an user is not verified, remove the user.
            if (existingUser != null)
            {
                throw new InvalidOperationException("A user with the given email already exists.");
            }
            var appUser = new ApplicationUser
            {
                UserName = email,
                Email = email,
                UserProfile = new UserProfile()
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    //SexualityId = string.IsNullOrEmpty(userDTO.SexualityId) ? null : Guid.Parse(userDTO.SexualityId),
                    //EthnicityId = string.IsNullOrEmpty(userDTO.EthnicityId) ? null : Guid.Parse(userDTO.EthnicityId),
                },
            };
            var createResult = await _authHandler.UserManager.CreateAsync(appUser, request.Password);

            if (createResult.Succeeded)
            {
                var addToRoleResult = await _authHandler.UserManager.AddToRoleAsync(appUser, "Student");
                if (addToRoleResult.Succeeded)
                {
                    await SendVerificationEmailAsync(appUser, url, protocol);
                    return appUser;
                }

                await _authHandler.UserManager.DeleteAsync(appUser);
                throw new InvalidOperationException("User registration failed.");
            }

            throw new InvalidOperationException(createResult.Errors.Select(e => e.Description).FirstOrDefault());
        }

        private async Task SendVerificationEmailAsync(ApplicationUser appUser, IUrlHelper url, string protocol)
        {
            var emailVerificationToken = await _authHandler.UserManager.GenerateEmailConfirmationTokenAsync(appUser);
            var verificationUrl = url.Action(action: "VerifyEmail", controller: "VerifyEmail", values: new { id = appUser.Id, token = emailVerificationToken.Base64Encode() }, protocol);
            if (appUser.UserName != null && appUser.Email != null && verificationUrl != null)
            {
                var message = new EmailService.Message(new Dictionary<string, string> { { appUser.UserName, appUser.Email } }, "Email Verification", verificationUrl);
                _authHandler.EmailSender.SendEmail(message, "Meet Campus");
            }
        }
    }
    public sealed class Validator : AbstractValidator<CreateUserRequest>
    {
        public Validator()
        {
            RuleFor(n => n.Email).NotEmpty().EmailAddress();
            RuleFor(n => n.Password).NotEmpty();
            RuleFor(n => n.PasswordConfirm).NotEmpty().WithMessage("Password confirmation is required.").Equal(n => n.Password).WithMessage("Password confirmation must match the password.");
            RuleFor(n => n.FirstName).NotEmpty();
            RuleFor(n => n.LastName).NotEmpty();
        }
    }

    public class CreateUserController : UserController
    {
        private readonly CreateUserHandler _handler;
        private readonly IValidator<CreateUserRequest> _validator;
        public CreateUserController(CreateUserHandler handler, IValidator<CreateUserRequest> validator)
        {
            _handler = handler;
            _validator = validator;
        }
        [HttpPost]
        [SwaggerOperation(Tags = new[] { "User" })]
        public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
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
                var result = await _handler.HandleAsync(request, Url, HttpContext.Request.Scheme);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, instance: null, StatusCodes.Status400BadRequest, title: "Register Error", type: "Bad Request");
            }
        }
    }
}
