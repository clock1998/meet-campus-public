using EmailService;
using Microsoft.AspNetCore.Identity;
using WebAPI.Features.Users;
using WebAPI.Infrastructure.Context;

namespace WebAPI.Features.Auth
{
    public class AuthHandler : Handler
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        public AuthHandler(AppDbContext context, IConfiguration configuration, UserManager<ApplicationUser> userManager, IEmailSender emailSender) : base(context)
        {
            _configuration = configuration;
            _userManager = userManager;
            _emailSender = emailSender;
        }

        public IConfiguration Configuration {  get { return _configuration; } }
        public UserManager<ApplicationUser> UserManager { get { return _userManager; } }
        public IEmailSender EmailSender { get { return _emailSender; } }
        public AppDbContext Context { get { return _context; } }
    }
}
