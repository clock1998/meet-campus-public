using WebAPI.Features.Auth;
using WebAPI.Infrastructure.Helper;

namespace WebAPI.Features.Email
{
    public class SendVerificationEmailHandler(AuthHandler _authHandler, LinkGenerator linkGenerator, IHttpContextAccessor httpContextAccessor)
    {
        public async Task HandleAsync(ApplicationUser appUser)
        {
            var emailVerificationToken = await _authHandler.UserManager.GenerateEmailConfirmationTokenAsync(appUser);
            var verificationUrl = linkGenerator.GetUriByName(httpContextAccessor.HttpContext!, "VerifyEmail", new { id = appUser.Id, token = emailVerificationToken.Base64Encode() });
            if (appUser.UserName != null && appUser.Email != null && verificationUrl != null)
            {
                var message = new EmailService.Message(new Dictionary<string, string> { { appUser.UserName, appUser.Email } }, "Email Verification", verificationUrl);
                _authHandler.EmailSender.SendEmail(message, "Meet Campus");
            }
        }
    }
}
