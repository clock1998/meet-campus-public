using EmailService;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Features.UserProfiles;
using WebAPI.Infrastructure.Helper;

namespace WebAPI.Features.Auth.Command
{
    public sealed record RegisterRequest(string Email, string Password, string PasswordConfirm, string FirstName, string LastName);
    public class RegisterHandler(AuthHandler _authHandler)
    {
        public async Task<ApplicationUser> HandleAsync(RegisterRequest request)
        {
            var email = request.Email.ToLower().Trim();
            var domain = email.Trim().Split("@")[1];

            //check if email is allowed
            if (!_authHandler.Context.Domains.Any(n => n.Record == domain))
            {
                throw new Exception($"Domains allowed: {string.Join(",", _authHandler.Context.Domains.Select(n => n.Record))}");
            }

            var existingUser = await _authHandler.UserManager.FindByEmailAsync(request.Email);
            //if an user is not verified, remove the user.
            if (existingUser != null && !existingUser.EmailConfirmed)
            {
                await _authHandler.UserManager.DeleteAsync(existingUser);
            }
            else if (existingUser != null)
            {
                // Check if a user with the same email already exists
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
    public sealed class Validator : AbstractValidator<RegisterRequest>
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
}
