// LoanWise.Application/Features/Borrowers/Queries/GetRiskSummary/GetBorrowerRiskSummaryQueryHandler.cs
using MediatR;
using LoanWise.Application.Common.Interfaces;
using LoanWise.Application.DTOs.Borrowers;
using StoreBoost.Application.Common.Models;

namespace LoanWise.Application.Features.Borrowers.Queries.GetRiskSummary;

public sealed class GetBorrowerRiskSummaryQueryHandler
    : IRequestHandler<GetBorrowerRiskSummaryQuery, ApiResponse<BorrowerRiskSummaryDto>>
{
    private readonly ICreditScoringService _score;
    private readonly IKycService _kyc;

    public GetBorrowerRiskSummaryQueryHandler(ICreditScoringService score, IKycService kyc)
    {
        _score = score; _kyc = kyc;
    }

    public async Task<ApiResponse<BorrowerRiskSummaryDto>> Handle(GetBorrowerRiskSummaryQuery q, CancellationToken ct)
    {
        var s = await _score.GetScoreAsync(q.BorrowerId, ct);
        var k = await _kyc.GetStatusAsync(q.BorrowerId, ct);

        var tier = s.Score >= 720 ? "Low"
                 : s.Score >= 640 ? "Medium"
                 : "High";

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
