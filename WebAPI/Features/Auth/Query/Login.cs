using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Infrastructure.Helper;

namespace WebAPI.Features.Auth.Query
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
        private readonly IConfiguration _configuration;
        public LoginHandler(AuthHandler authHandler, IConfiguration configuration)
        {
            _authHandler = authHandler;
            _configuration = configuration;
        }

        public async Task<AppContextResponse> HandleAsync(LoginRequest request)
        {
            var user = await _authHandler.UserManager.FindByEmailAsync(request.Username);
            var jwtSettings = _configuration.GetSection("Jwt");
            if (user != null)
            {
                var result = await _authHandler.UserManager.CheckPasswordAsync(user, request.Password);
                var isEmailConfirmed = await _authHandler.UserManager.IsEmailConfirmedAsync(user);
                if (result)
                {
                    if (isEmailConfirmed)
                    {
                        user.RefreshToken = AuthHelper.CreateRefreshToken();
                        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(int.Parse(jwtSettings["RefreshTokenExp"]!));
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
}
