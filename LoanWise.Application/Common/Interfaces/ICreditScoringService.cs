// LoanWise.Application/Common/Interfaces/ICreditScoringService.cs
namespace LoanWise.Application.Common.Interfaces;

public interface ICreditScoringService
{
    Task<CreditScoreResult> GetScoreAsync(Guid borrowerId, CancellationToken ct = default);
}

public sealed record CreditScoreResult(Guid BorrowerId, int Score, string Provider, DateTime GeneratedAtUtc);
