using global::LoanWise.Application.Common.Interfaces;
using global::LoanWise.Application.DTOs.Auth;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StoreBoost.Application.Common.Models; 

namespace LoanWise.Application.Features.Auth.Commands.RefreshToken
{
    /// <summary>
    /// Handler:
    /// 1) Hash incoming refresh token via ITokenService.Hash
    /// 2) Load RefreshToken by hash; validate not revoked/expired
    /// 3) Load User
    /// 4) Rotate: revoke old, create new refresh token (store hash)
    /// 5) Issue new access token via ITokenService.CreateAccessToken
    /// 6) Return ApiResponse<RefreshTokenResponse>
    /// </summary>
    public sealed class RefreshTokenCommandHandler
        : IRequestHandler<RefreshTokenCommand, ApiResponse<RefreshTokenResponse>>
    {
        private readonly IApplicationDbContext _db;
        private readonly ITokenService _tokens;

        public RefreshTokenCommandHandler(IApplicationDbContext db, ITokenService tokens)
        {
            _db = db;
            _tokens = tokens;
        }

        public async Task<ApiResponse<RefreshTokenResponse>> Handle(
            RefreshTokenCommand request,
            CancellationToken cancellationToken)
        {
            var rawRefresh = request.Request.RefreshToken?.Trim();
            if (string.IsNullOrWhiteSpace(rawRefresh))
                return ApiResponse<RefreshTokenResponse>.FailureResult("Missing refresh token.");

            var refreshHash = _tokens.Hash(rawRefresh);

            // Load stored (hashed) refresh token
            var rt = await _db.RefreshTokens
                .AsTracking()
                .FirstOrDefaultAsync(x => x.TokenHash == refreshHash, cancellationToken);

            if (rt is null)
                return ApiResponse<RefreshTokenResponse>.FailureResult("Invalid refresh token.");

            // Basic validity checks
            var now = DateTime.UtcNow;
            var isExpired = now > rt.ExpiresAtUtc;
            var isRevoked = rt.RevokedAtUtc.HasValue;

            if (isRevoked || isExpired)
                return ApiResponse<RefreshTokenResponse>.FailureResult("Refresh token expired or revoked.");

            // Load the user
            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.Id == rt.UserId, cancellationToken);

            if (user is null)
                return ApiResponse<RefreshTokenResponse>.FailureResult("User not found.");

            // Rotate: revoke the old token
            rt.RevokedAtUtc = now;

            // Create a new refresh token (raw for client, hash for DB)
            var newRefreshRaw = _tokens.CreateRefreshToken(out var newRefreshExpiry);
            var newRefreshHash = _tokens.Hash(newRefreshRaw);

            rt.ReplacedByTokenHash = newRefreshHash;

            var rotated = new LoanWise.Domain.Entities.RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                TokenHash = newRefreshHash,
                CreatedAtUtc = now,
                ExpiresAtUtc = newRefreshExpiry,
                CreatedByIp = request.IpAddress ?? "unknown"
            };

            // Create a new access token
            var access = _tokens.CreateAccessToken(user, out var accessExpiry);

            _db.RefreshTokens.Update(rt);
            await _db.RefreshTokens.AddAsync(rotated, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);

            var payload = new RefreshTokenResponse(
                Token: access,
                TokenExpiresAtUtc: accessExpiry,
                RefreshToken: newRefreshRaw,                // raw only ever returned to client
                RefreshTokenExpiresAtUtc: newRefreshExpiry
            );

            return ApiResponse<RefreshTokenResponse>.SuccessResult(payload, "Token refreshed.");
        }
    }
}
