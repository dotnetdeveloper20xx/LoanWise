using LoanWise.Application.DTOs.Auth;
using MediatR;
using StoreBoost.Application.Common.Models;

namespace LoanWise.Application.Features.Auth.Commands.RefreshToken
{
    /// <summary>
    /// Issues a new access token using a valid refresh token, and rotates the refresh token.
    /// </summary>
    public sealed record RefreshTokenCommand(RefreshTokenRequest Request, string? IpAddress)
        : IRequest<ApiResponse<RefreshTokenResponse>>;
}
