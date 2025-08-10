// LoanWise.Application/Features/Borrowers/Commands/VerifyKyc/VerifyKycCommandHandler.cs
using MediatR;
using LoanWise.Application.Common.Interfaces;
using StoreBoost.Application.Common.Models;

namespace LoanWise.Application.Features.Borrowers.Commands.VerifyKyc;

public sealed class VerifyKycCommandHandler : IRequestHandler<VerifyKycCommand, ApiResponse<string>>
{
    private readonly IKycService _kyc;
    public VerifyKycCommandHandler(IKycService kyc) => _kyc = kyc;

    public async Task<ApiResponse<string>> Handle(VerifyKycCommand request, CancellationToken ct)
    {
        var res = await _kyc.VerifyAsync(request.BorrowerId, ct);
        return ApiResponse<string>.SuccessResult(res.Status, $"KYC {res.Status} at {res.VerifiedAtUtc:u}");
    }
}
