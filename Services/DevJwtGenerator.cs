using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace PortfolioAnalyticsApi.Services{

    public static class DevJwtGenerator
    {
        public static string GenerateToken()
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, "dev-user-123"),
                new Claim(JwtRegisteredClaimNames.Email, "dev@local.test"),
                new Claim("role", "AnalyticsViewer")
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes("DEV_ONLY_SUPER_SECRET_KEY_123456789")
            );

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "https://dev-auth",
                audience: "analytics-api",
                claims: claims,
                expires: DateTime.UtcNow.AddDays(30),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}