// LoanWise.Infrastructure/Credit/MockCreditScoringService.cs
using System.Security.Cryptography;
using LoanWise.Application.Common.Interfaces;

namespace LoanWise.Infrastructure.Credit;

public sealed class MockCreditScoringService : ICreditScoringService
{
    public Task<CreditScoreResult> GetScoreAsync(Guid borrowerId, CancellationToken ct = default)
    {
        // Deterministic 550–850 based on borrowerId
        var score = 550 + (Stable0to1(borrowerId) * 300); // 550..850
        var rounded = (int)Math.Round(score / 5.0, MidpointRounding.AwayFromZero) * 5; // nicer buckets

        return Task.FromResult(new CreditScoreResult(
            borrowerId, rounded, "LoanWise.MockScore v1", DateTime.UtcNow));
    }

    private static double Stable0to1(Guid id)
    {
        var h = SHA256.HashData(id.ToByteArray());
        var u = BitConverter.ToUInt32(h, 0);
        return u / (double)uint.MaxValue;
    }
}
