using LoanWise.Domain.Entities;

namespace LoanWise.Application.Common.Interfaces
{
    public interface ITokenService
    {
        string CreateAccessToken(User user, out DateTime expiresAtUtc);
        string CreateRefreshToken(out DateTime expiresAtUtc); // returns raw token
        string Hash(string raw);
    }
}
