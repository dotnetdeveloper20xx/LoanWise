using System.Security.Cryptography;
using System.Text;
using LoanWise.Application.Common.Interfaces;
using LoanWise.Domain.Entities;
using Microsoft.Extensions.Configuration;

namespace LoanWise.Infrastructure.Auth
{
    public sealed class JwtTokenService : ITokenService
    {
        private readonly IJwtTokenGenerator _jwt; 
        private readonly IConfiguration _cfg;

        public JwtTokenService(IJwtTokenGenerator jwt, IConfiguration cfg)
        {
            _jwt = jwt;
            _cfg = cfg;
        }

        public string CreateAccessToken(User user, out DateTime expiresAtUtc)
        {
            // Let your generator build the token; take expiry from config
            var minutes = int.TryParse(_cfg["Jwt:AccessTokenMinutes"], out var m) ? m : 60;
            expiresAtUtc = DateTime.UtcNow.AddMinutes(minutes);
            return _jwt.GenerateToken(user);
        }

        public string CreateRefreshToken(out DateTime expiresAtUtc)
        {
            // 64 bytes -> Base64 string (cryptographically strong)
            var bytes = RandomNumberGenerator.GetBytes(64);
            var raw = Convert.ToBase64String(bytes);

            var days = int.TryParse(_cfg["Jwt:RefreshTokenDays"], out var d) ? d : 30;
            expiresAtUtc = DateTime.UtcNow.AddDays(days);

            return raw; // return raw for the client (store only the hash server-side)
        }

        public string Hash(string raw)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(raw));
            return Convert.ToBase64String(bytes);
        }
    }
}
