using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Features.Auth;
using WebAPI.Features.Email;
using WebAPI.Features.UserProfiles;

namespace WebAPI.Features.Users.Command
{
    public sealed record CreateUserRequest(string Email, string Password, string PasswordConfirm, string FirstName, string LastName);
    public sealed record CreateUserResponse(Guid Id, int Year, string Name);
    public class CreateUserHandler(AuthHandler _authHandler)
    {
        public async Task<ApplicationUser> HandleAsync(CreateUserRequest request)
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
                    return appUser;
                }

                await _authHandler.UserManager.DeleteAsync(appUser);
                throw new InvalidOperationException("User registration failed.");
            }

            throw new InvalidOperationException(createResult.Errors.Select(e => e.Description).FirstOrDefault());
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
        private readonly SendVerificationEmailHandler _sendVerificationEmailHandler;
        private readonly IValidator<CreateUserRequest> _validator;
        public CreateUserController(CreateUserHandler handler, IValidator<CreateUserRequest> validator, SendVerificationEmailHandler sendVerificationEmailHandler)
        {
            _handler = handler;
            _validator = validator;
            _sendVerificationEmailHandler = sendVerificationEmailHandler;
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
                var user = await _handler.HandleAsync(request);
                if(user is not null)
                {
                    await _sendVerificationEmailHandler.HandleAsync(user);
                }
                return Ok(user);
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, instance: null, StatusCodes.Status400BadRequest, title: "Register Error", type: "Bad Request");
            }
        }
    }
}
