using LoanWise.Application.Common.Interfaces;
using LoanWise.Application.Features.Borrowers.Commands.VerifyKyc;
using LoanWise.Domain.Entities;
using MediatR;
using StoreBoost.Application.Common.Models;
using System.Text.Json;
// ...

public sealed class VerifyKycCommandHandler : IRequestHandler<VerifyKycCommand, ApiResponse<string>>
{
    private readonly IKycService _kyc;
    private readonly ICreditScoringService _score;
    private readonly IBorrowerRiskRepository _repo;

    public VerifyKycCommandHandler(IKycService kyc, ICreditScoringService score, IBorrowerRiskRepository repo)
    {
        _kyc = kyc; _score = score; _repo = repo;
    }

    public async Task<ApiResponse<string>> Handle(VerifyKycCommand request, CancellationToken ct)
    {
        var res = await _kyc.VerifyAsync(request.BorrowerId, ct);
        var sc = await _score.GetScoreAsync(request.BorrowerId, ct);

        var tier = sc.Score >= 720 ? "Low" : sc.Score >= 640 ? "Medium" : "High";
        var snapshot = new BorrowerRiskSnapshot
        {
            BorrowerId = request.BorrowerId,
            CreditScore = sc.Score,
            RiskTier = tier,
            KycStatus = res.Status,
            FlagsJson = JsonSerializer.Serialize(res.Flags),
            LastVerifiedAtUtc = res.VerifiedAtUtc,
            LastScoreAtUtc = sc.GeneratedAtUtc,
            UpdatedAtUtc = DateTime.UtcNow
        };

        await _repo.UpsertAsync(snapshot, ct);

        return ApiResponse<string>.SuccessResult(res.Status, $"KYC {res.Status} at {res.VerifiedAtUtc:u}");
    }
}
