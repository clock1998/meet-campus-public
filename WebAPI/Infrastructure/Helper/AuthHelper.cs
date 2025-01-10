using Microsoft.IdentityModel.Tokens;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using WebAPI.Features.Auth;

namespace WebAPI.Infrastructure.Helper
{
    public static class AuthHelper
    {
        private static RsaSecurityKey RsaKey
        {
            get
            {
                var rsaKey = RSA.Create();
                rsaKey.ImportRSAPrivateKey(File.ReadAllBytes("key"), out _);
                return new RsaSecurityKey(rsaKey);
            }
        }

        public static string CreateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        public static string CreateToken(ApplicationUser user, List<string> roles, IConfiguration configuration)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            };
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            //var jwtToken = new JwtSecurityToken(
            //    _jwtTokenConfig.Issuer,
            //    shouldAddAudienceClaim ? _jwtTokenConfig.Audience : string.Empty,
            //    claims,
            //    expires: now.AddMinutes(_jwtTokenConfig.AccessTokenExpiration),
            //    signingCredentials: new SigningCredentials(new SymmetricSecurityKey(_secret), SecurityAlgorithms.HmacSha256Signature));
            //var accessToken = new JwtSecurityTokenHandler().WriteToken(jwtToken);
            var jwtSettings = configuration.GetSection("Jwt");

            var jwtToken = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims,
                //expires: DateTime.UtcNow.AddSeconds(10),
                expires: DateTime.UtcNow.AddHours(int.Parse(jwtSettings["exp"]!)),
                signingCredentials: new SigningCredentials(RsaKey, SecurityAlgorithms.RsaSha256)
            );
            var accessToken = new JwtSecurityTokenHandler().WriteToken(jwtToken);
            return accessToken;
        }

        public static ClaimsPrincipal GetPrincipalFromExpiredToken(string token, IConfiguration configuration)
        {
            var jwtSettings = configuration.GetSection("Jwt");
            var issuer = jwtSettings["Issuer"];

            var tokenValidationParameters = new TokenValidationParameters
            {
                //ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],
                ValidateIssuer = false,
                ValidateIssuerSigningKey = false,
                IssuerSigningKey = RsaKey,
                ValidateLifetime = false //here we are saying that we don't care about the token's expiration date
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;
            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.RsaSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");
            return principal;
        }
    }
}
