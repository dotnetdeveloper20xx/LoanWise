// LoanWise.Infrastructure/Kyc/MockKycService.cs
using System.Security.Cryptography;
using LoanWise.Application.Common.Interfaces;
using Microsoft.Extensions.Options;

namespace LoanWise.Infrastructure.Kyc;

public sealed class MockKycService : IKycService
{
    private readonly KycOptions _opts;
    private static readonly string[] _passFlags = Array.Empty<string>();
    private static readonly string[] _failFlags = new[] { "DocumentMismatch", "AddressUnverified" };

    public MockKycService(IOptions<KycOptions> options) => _opts = options.Value;

    public Task<KycResult> VerifyAsync(Guid borrowerId, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var passed = StablePass(borrowerId, _opts.PassRate);
        var status = passed ? "Verified" : "Failed";
        var flags = passed ? _passFlags : _failFlags;

        return Task.FromResult(new KycResult(borrowerId, status, flags, now));
    }

    public Task<KycSnapshot> GetStatusAsync(Guid borrowerId, CancellationToken ct = default)
    {
        // For mock, derive a deterministic status without persistence
        var passed = StablePass(borrowerId, _opts.PassRate);
        var status = passed ? "Verified" : "Pending"; // show "Pending" until admin triggers Verify
        var flags = passed ? _passFlags : _failFlags;

        return Task.FromResult(new KycSnapshot(borrowerId, status, flags, null));
    }

    private static bool StablePass(Guid id, double passRate)
    {
        // Deterministic pseudo-random by borrowerId
        var hash = SHA256.HashData(id.ToByteArray());
        var val = BitConverter.ToUInt32(hash, 0) / (double)uint.MaxValue; // 0..1
        return val <= passRate;
    }
}
