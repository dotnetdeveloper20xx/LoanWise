using LoanWise.Application.Common.Interfaces;
using LoanWise.Application.DTOs.Borrowers;
using LoanWise.Application.Features.Borrowers.Queries.GetRiskSummary;
using LoanWise.Domain.Entities;
using MediatR;
using StoreBoost.Application.Common.Models;
using System.Text.Json;
// ...

public sealed class GetBorrowerRiskSummaryQueryHandler
    : IRequestHandler<GetBorrowerRiskSummaryQuery, ApiResponse<BorrowerRiskSummaryDto>>
{
    private readonly ICreditScoringService _score;
    private readonly IKycService _kyc;
    private readonly IBorrowerRiskRepository _repo;

    public GetBorrowerRiskSummaryQueryHandler(ICreditScoringService score, IKycService kyc, IBorrowerRiskRepository repo)
    {
        _score = score; _kyc = kyc; _repo = repo;
    }

    public async Task<ApiResponse<BorrowerRiskSummaryDto>> Handle(GetBorrowerRiskSummaryQuery q, CancellationToken ct)
    {
        var s = await _score.GetScoreAsync(q.BorrowerId, ct);         // recompute (mock deterministic)
        var k = await _kyc.GetStatusAsync(q.BorrowerId, ct);           // status/flags (may be Pending)

        var tier = s.Score >= 720 ? "Low" : s.Score >= 640 ? "Medium" : "High";

        // Merge with existing snapshot so we don't lose LastVerifiedAtUtc unless explicitly verified
        var existing = await _repo.GetAsync(q.BorrowerId, ct);
        var snapshot = new BorrowerRiskSnapshot
        {
            BorrowerId = q.BorrowerId,
            CreditScore = s.Score,
            RiskTier = tier,
            KycStatus = k.Status,
            FlagsJson = JsonSerializer.Serialize(k.Flags),
            LastVerifiedAtUtc = existing?.LastVerifiedAtUtc, // preserve verification time
            LastScoreAtUtc = s.GeneratedAtUtc,
            UpdatedAtUtc = DateTime.UtcNow
        };
        await _repo.UpsertAsync(snapshot, ct);

        var dto = new BorrowerRiskSummaryDto
        {
            BorrowerId = q.BorrowerId,
            CreditScore = s.Score,
            RiskTier = tier,
            KycStatus = k.Status,
            Flags = k.Flags,
            GeneratedAtUtc = s.GeneratedAtUtc
        };

        return ApiResponse<BorrowerRiskSummaryDto>.SuccessResult(dto);
    }
}
