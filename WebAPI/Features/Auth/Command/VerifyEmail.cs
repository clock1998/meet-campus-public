using EmailService;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Features.Users;
using WebAPI.Infrastructure.Context;
using WebAPI.Infrastructure.Helper;

namespace WebAPI.Features.Auth.Command
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
}
