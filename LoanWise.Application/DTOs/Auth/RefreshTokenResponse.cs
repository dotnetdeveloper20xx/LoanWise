namespace LoanWise.Application.DTOs.Auth
{
    public record RefreshTokenResponse(string Token, DateTime TokenExpiresAtUtc,
                                    string RefreshToken, DateTime RefreshTokenExpiresAtUtc);
}
