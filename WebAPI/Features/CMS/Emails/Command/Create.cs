using Core;
using EmailService;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Infrastructure.Context;

namespace WebAPI.Features.CMS.Emails.Command
{
    public sealed record CreateEmailRequest(string EmailAdress, string CfTurnstileResponse);
    public class CreateEmailHandler : Handler
    {
        private readonly IEmailSender _emailSender;
        public CreateEmailHandler(AppDbContext context, IEmailSender emailSender) : base(context)
        {
            _emailSender = emailSender;
        }

        public async Task<Email> HandleAsync(CreateEmailRequest request)
        {
            var email = new Email
            {
                EmailAdress = request.EmailAdress.ToLower().Trim(),
            };
            var existingEmail = await _context.Emails.SingleOrDefaultAsync(n => n.EmailAdress.Equals(email.EmailAdress));
            if (existingEmail != null)
            {
                throw new InvalidOperationException("Email is registered.");
            }
            var emailBody = "<h1>Thank you for your support!</h1>\r\n<p>Regards,</p>\r\n<p>Meet Campus</p>";
            var message = new Message(new Dictionary<string, string> { { "", email.EmailAdress } }, "Meet Campus", emailBody);
            _emailSender.SendEmail(message, "Meet Campus");
            return email;
            //catch (DbUpdateException ex)
            //{
            //    if (ex.InnerException != null)
            //    {
            //        PostgresException innerEx = (PostgresException)ex.InnerException;

            //        if (innerEx.SqlState == "23505")
            //        {
            //            return Problem(detail: "Email is used.", instance: null, 400, title: "Email Signup", type: "Email Signup");
            //        }
            //    }
            //    return Problem(detail: ex.Message, instance: null, 400, title: "Email Signup", type: "Email Signup");
            //}
        }
    }

    public sealed class Validator : AbstractValidator<CreateEmailRequest>
    {
        public Validator()
        {
            RuleFor(n => n.EmailAdress).NotEmpty();
            RuleFor(n => n.CfTurnstileResponse).NotEmpty();
        }
    }

    [ApiController, Route("api/email")]
    public class CreateController : ControllerBase
    {
        private readonly CreateEmailHandler _handler;
        private readonly IValidator<CreateEmailRequest> _validator;
        private CloudflareSiteVerifyClient cloudflareSiteVerifyClient;
        public CreateController(CreateEmailHandler handler, IValidator<CreateEmailRequest> validator)
        {
            _handler = handler;
            _validator = validator;
            cloudflareSiteVerifyClient = new CloudflareSiteVerifyClient();
        }

        [SwaggerOperation(Tags = new[] { "Email" })]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateEmailRequest request)
        {
            var validatorResult = await _validator.ValidateAsync(request);
            if (!validatorResult.IsValid) 
            {
                return Problem(detail: "Invalide input", instance: null, StatusCodes.Status400BadRequest, title: "Bad Request",
                    extensions: new Dictionary<string, object?>{
                        { "erros", validatorResult.Errors.Select(n => n.ErrorMessage).ToArray()} 
                    });
            }
            if (User.IsInRole("Admin") || await cloudflareSiteVerifyClient.VerifyAsync(HttpContext.Request, request.CfTurnstileResponse))
            {
                try
                {
                    var result = await _handler.HandleAsync(request);
                }
                catch (Exception ex)
                {
                    return Problem(detail: ex.Message, instance: null, StatusCodes.Status400BadRequest, title: "Bad Request");
                }
            }
            return Problem(detail: "Cloudflare Site Verification failed", instance: null, StatusCodes.Status400BadRequest, title: "Bad Request");
        }
    }
}
